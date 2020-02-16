using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR {
    public class NomaiVR: ModBehaviour {
        public static IModHelper Helper;
        static NomaiVR _instance;
        public static bool DebugMode;

        void Start () {
            _instance = this;

            Log("Start Main");

            Helper = ModHelper;

            SteamVR.Initialize();

            ShipTools.Patches.Patch();
            ControllerInput.Patches.Patch();
            Dialog.Patches.Patch();
            EffectFixes.Patches.Patch();
            HoldProbeLauncher.Patches.Patch();
            HoldSignalscope.Patches.Patch();
            HoldTranslator.Patches.Patch();
            HoldMallowStick.Patches.Patch();
            LaserPointer.Patches.Patch();
            PlayerBodyPosition.Patches.Patch();

            // Add all modules here.
            gameObject.AddComponent<Common>();
            gameObject.AddComponent<ControllerInput>();

            var gameModules = new GameObject();
            gameModules.AddComponent<Menus>();

            Application.runInBackground = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable () {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode) {

            var isSolarSystem = scene.name == "SolarSystem";
            var isEye = scene.name == "EyeOfTheUniverse";
            var isTitle = scene.name == "TitleScreen";

            // The GameObject associated with this ModBehaviour is set to persist between scene loads.
            // Some modules need to be restarted on every scene load.
            // This GameObject is for them.
            var nonPersistentObject = new GameObject();

            if (isSolarSystem || isEye) {
                Common.InitGame();
                nonPersistentObject.AddComponent<EffectFixes>();
                nonPersistentObject.AddComponent<PlayerBodyPosition>();
                nonPersistentObject.AddComponent<Dialog>();
                nonPersistentObject.AddComponent<Hands>();
                if (isSolarSystem) {
                    nonPersistentObject.AddComponent<ShipTools>();
                }
            } else if (isTitle) {
                Menus.Reset();
            }

            nonPersistentObject.AddComponent<Menus>().isInGame = isSolarSystem || isEye;
        }

        public override void Configure (IModConfig config) {
            DebugMode = config.GetSettingsValue<bool>("debugMode");
            XRSettings.showDeviceView = config.GetSettingsValue<bool>("showMirrorView");
            // Prevent application from stealing mouse focus;
            ModHelper.HarmonyHelper.EmptyMethod<CursorManager>("Update");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void Log (params string[] strings) {
            if (DebugMode && Helper != null) {
                Helper.Console.WriteLine(string.Join(" ", strings));
            }
        }

        public static void Pre<T> (string methodName, Type patchType, string patchMethodName) {
            Helper.HarmonyHelper.AddPrefix<T>(methodName, patchType, patchMethodName);
        }

        public static void Post<T> (string methodName, Type patchType, string patchMethodName) {
            Helper.HarmonyHelper.AddPostfix<T>(methodName, patchType, patchMethodName);
        }

        public static void Empty<T> (string methodName) {
            Helper.HarmonyHelper.EmptyMethod<T>(methodName);
        }
    }
}
