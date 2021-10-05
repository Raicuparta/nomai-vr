using Valve.VR;
using BepInEx;
using System.IO;
using HarmonyLib;
using NomaiVR.Input;
using NomaiVR.Tools;
using NomaiVR.UI;

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
            Save = ModSaveFile.LoadSaveFile();
            new FatalErrorChecker();
            new AssetLoader();

            InitSteamVR();
            
            //// Load all modules.
            //// I'm sorry to say that order does matter here.
            new ForceSettings();
            //new ControllerInput();
            new NewControllerInput();
            new Dialogue();
            new FogFix();
            new ShadowsFix();
            new LoopTransitionFix();
            new VisorEffectsFix();
            new DreamLanternFix();
            new ScreenSpaceReflectionDisabler();
            new ProjectionStoneCameraFix();
            new CameraMaskFix();
            new MapFix();
            new PlayerBodyPosition();
            new VRToolSwapper();
            new HandsController();
            new ShipTools();
            new FlashlightGesture();
            new HoldMallowStick();
            new HoldProbeLauncher();
            new HoldSignalscope();
            new HoldTranslator();
            new HoldItem();
            new HoldPrompts();
            new LaserPointer();
            new FeetMarker();
            new HelmetHUD();
            //new InputPrompts();
            new NewInputPrompts();
            new RemoveUnusedInputPrompts();
            new ControllerModels();
            new GesturePrompts();
            new PostCreditsFix();
            new LookArrow();
            new DisableDeathAnimation();
            new Menus();
            new DebugCheats();
        }

        private void InitSteamVR()
        {
            try
            {
                SteamVR_Actions.PreInitialize();
                SteamVR.Initialize();
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;

                ApplicationManifestHelper.UpdateManifest(ModFolderPath + @"\assets\outerwilds.vrmanifest",
                                                        "steam.app.753640",
                                                        "https://steamcdn-a.akamaihd.net/steam/apps/753640/header.jpg",
                                                        "Outer Wilds VR",
                                                        "NomaiVR mod for Outer Wilds",
                                                        steamBuild: SteamManager.Initialized,
                                                        steamAppId: 753640);

                OpenVR.Input.SetActionManifestPath(ModFolderPath + @"\bindings\actions.json");
            }
            catch
            {
                FatalErrorChecker.ThrowSteamVRError();
            }
        }
    }
}
