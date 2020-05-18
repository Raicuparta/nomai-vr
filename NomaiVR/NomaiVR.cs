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

        void Start()
        {
            Log("Start NomaiVR");

            SaveFile = ModHelper.Storage.Load<ModSaveFile>(ModSaveFile.FileName);
            Helper = ModHelper;

            SteamVR.Initialize();

            // Load all modules.
            // I'm sorry to say that order does matter here.
            new AssetLoader();
            new ForceSettings();
            new ControllerInput();
            new Dialogue();
            new FogFix();
            new LoopTransitionFix();
            new WaterFix();
            new ProjectionStoneCameraFix();
            new CameraMaskFix();
            new MapFix();
            new PlayerBodyPosition();
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
            new InputPrompts();
            new VRTutorial();
            new PostCreditsFix();
            new Menus();
        }

        public override void Configure(IModConfig config)
        {
            DebugMode = config.GetSettingsValue<bool>("debugMode");
            RefreshRate = config.GetSettingsValue<int>("overrideRefreshRate");
            XRSettings.showDeviceView = config.GetSettingsValue<bool>("showMirrorView");

            if (config.GetSettingsValue<bool>("preventCursorLock"))
            {
                ModHelper.HarmonyHelper.EmptyMethod<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public static void Log(params object[] strings)
        {
            if (DebugMode && Helper != null)
            {
                Helper.Console.WriteLine(strings);
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
