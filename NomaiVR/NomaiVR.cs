using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper;
        public static bool DebugMode;
        public static int RefreshRate;
        public static ModSaveFile SaveFile;
        public static GameObject persistentParent;
        public static GameObject nonPersistentParent;

        void Start()
        {
            Log("Start Main");

            persistentParent = gameObject;

            SaveFile = ModHelper.Storage.Load<ModSaveFile>(ModSaveFile.FileName);
            Helper = ModHelper;

            SteamVR.Initialize();

            // Load all modules.
            // I'm sorry to say that order does matter here.
            new ForceSettings();
            new ShipTools();
            new ControllerInput();
            new Dialogue();
            new FogFix();
            new LoopTransitionFix();
            new WaterFix();
            new ProjectionStoneCameraFix();
            new CameraMaskFix();
            new MapFix();
            new HandsController();
            new FlashlightGesture();
            HoldProbeLauncher.Patches.Patch();
            HoldSignalscope.Patches.Patch();
            HoldTranslator.Patches.Patch();
            HoldMallowStick.Patches.Patch();
            LaserPointer.Patches.Patch();
            new PlayerBodyPosition();
            new FeetMarker();
            new HelmetHUD();
            new InputPrompts();
            new VRTutorial();
            new PostCreditsFix();
            new Menus();

            Application.runInBackground = true;

            LoadManager.OnCompleteSceneLoad += OnSceneLoaded;
        }

        void OnSceneLoaded(OWScene originalScene, OWScene scene)
        {
            var isPostCredits = scene == OWScene.PostCreditsScene;

            // The GameObject associated with this ModBehaviour is set to persist between scene loads.
            // Some modules need to be restarted on every scene load.
            // This GameObject is for them.
            nonPersistentParent = new GameObject();
        }

        public override void Configure(IModConfig config)
        {
            DebugMode = config.GetSettingsValue<bool>("debugMode");
            RefreshRate = config.GetSettingsValue<int>("overrideRefreshRate");
            XRSettings.showDeviceView = config.GetSettingsValue<bool>("showMirrorView");
            // Prevent application from stealing mouse focus;
            ModHelper.HarmonyHelper.EmptyMethod<CursorManager>("Update");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void Log(params string[] strings)
        {
            if (DebugMode && Helper != null)
            {
                Helper.Console.WriteLine(string.Join(" ", strings));
            }
        }

        public static void Pre<T>(string methodName, Type patchType, string patchMethodName)
        {
            Helper.HarmonyHelper.AddPrefix<T>(methodName, patchType, patchMethodName);
        }

        public static void Post<T>(string methodName, Type patchType, string patchMethodName)
        {
            Helper.HarmonyHelper.AddPostfix<T>(methodName, patchType, patchMethodName);
        }

        public static void Empty<T>(string methodName)
        {
            Helper.HarmonyHelper.EmptyMethod<T>(methodName);
        }
    }
}
