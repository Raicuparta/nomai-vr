using Valve.VR;
using BepInEx;
using System.IO;
using HarmonyLib;
using NomaiVR.Input;
using UnityEngine.XR.Management;
using Unity.XR.OpenVR;

namespace NomaiVR
{
    [BepInPlugin("raicuparta.nomaivr", "NomaiVR", "2.0.0")]
    public class NomaiVR : BaseUnityPlugin
    {
        public static ModSaveFile Save;
        public static readonly string ModFolderPath = $"{Directory.GetCurrentDirectory()}/BepInEx/plugins/NomaiVR/";
        public static Harmony HarmonyInstance;

        internal void Awake()
        {
            HarmonyInstance = new Harmony("raicuparta.nomaivr");
            ModSettings.SetConfig(Config);
        }

        internal void Start()
        {
            RestoreLogging();
            LoadManager.OnCompleteSceneLoad += (original, newScene) => RestoreLogging();

            Save = ModSaveFile.LoadSaveFile();
            new FatalErrorChecker();
            new AssetLoader();

            InitSteamVR();

            // Load all modules.
            // I'm sorry to say that order does matter here.
            new ForceSettings();
            //new ControllerInput();
            new NewControllerInput();
            new Dialogue();
            new FogFix();
            new ShadowsFix();
            new LoopTransitionFix();
            new VisorEffectsFix();
            new ProjectionStoneCameraFix();
            new CameraMaskFix();
            new MapFix();
            new PlayerBodyPosition();
            //new VRToolSwapper();
            //new HandsController();
            //new ShipTools();
            //new FlashlightGesture();
            //new HoldMallowStick();
            //new HoldProbeLauncher();
            //new HoldSignalscope();
            //new HoldTranslator();
            //new HoldItem();
            //new HoldPrompts();
            //new LaserPointer();
            //new FeetMarker();
            //new HelmetHUD();
            //new InputPrompts();
            //new ControllerModels();
            //new GesturePrompts();
            new PostCreditsFix();
            //new LookArrow();
            //new DisableDeathAnimation();
            new Menus();
        }

        private void RestoreLogging()
        {
            UnityEngine.Debug.unityLogger.logEnabled = true;
            UnityEngine.Debug.unityLogger.filterLogType = UnityEngine.LogType.Error;
        }

        private void InitSteamVR()
        {
            try
            {
                SteamVR_Actions.PreInitialize();
                LoadXRModule();
                SteamVR.Initialize();
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
                OpenVR.Input.SetActionManifestPath(ModFolderPath + @"\bindings\actions.json");

                if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null
                    && XRGeneralSettings.Instance.Manager.activeLoader != null)
                {
                    XRGeneralSettings.Instance.Manager.StartSubsystems();
                }
                else
                    throw new System.Exception("Cannot initialize VRSubsystem");
            }
            catch
            {
                FatalErrorChecker.ThrowSteamVRError();
            }
        }

        private void LoadXRModule()
        {
            foreach(var xrManager in AssetLoader.XRManager.LoadAllAssets())
                Logs.WriteInfo($"Loaded xrManager: {xrManager.name}");

            XRGeneralSettings instance = XRGeneralSettings.Instance;
            if (instance == null) throw new System.Exception("XRGeneralSettings instance is null");

            var xrManagerSettings = instance.Manager;
            if(xrManagerSettings == null) throw new System.Exception("XRManagerSettings instance is null");

            xrManagerSettings.InitializeLoaderSync();
            if (xrManagerSettings.activeLoader == null) throw new System.Exception("Cannot initialize OpenVR Loader");

            OpenVRSettings openVrSettings = OpenVRSettings.GetSettings(false);
            openVrSettings.EditorAppKey = "steam.app.753640";
            openVrSettings.InitializationType = OpenVRSettings.InitializationTypes.Scene;
            if (openVrSettings == null) throw new System.Exception("OpenVRSettings instance is null");

            openVrSettings.SetMirrorViewMode(OpenVRSettings.MirrorViewModes.Right);
        }
    }
}
