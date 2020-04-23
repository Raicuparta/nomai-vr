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
        public static bool DebugMode;
        public static ModSaveFile SaveFile;

        void Start () {
            Log("Start Main");

            SaveFile = ModHelper.Storage.Load<ModSaveFile>(ModSaveFile.FileName);
            Helper = ModHelper;

            SteamVR.Initialize();

            ShipTools.Patches.Patch();
            ControllerInput.Patches.Patch();
            Dialogue.Patches.Patch();
            EffectFixes.Patches.Patch();
            HoldProbeLauncher.Patches.Patch();
            HoldSignalscope.Patches.Patch();
            HoldTranslator.Patches.Patch();
            HoldMallowStick.Patches.Patch();
            LaserPointer.Patches.Patch();
            PlayerBodyPosition.Patches.Patch();
            ForceSettings.Patches.Patch();
            HelmetHUD.Patches.Patch();
            InputPrompts.Patches.Patch();
            VRTutorial.Patches.Patch();
            Menus.Patches.Patch();

            // These components will remain active between scene loads.
            gameObject.AddComponent<Common>();
            gameObject.AddComponent<ControllerInput>();
            gameObject.AddComponent<ForceSettings>();

            var gameModules = new GameObject();
            gameModules.AddComponent<Menus>();

            Application.runInBackground = true;

            LoadManager.OnCompleteSceneLoad += OnSceneLoaded;
        }

        void OnSceneLoaded (OWScene originalScene, OWScene scene) {
            var isSolarSystem = scene == OWScene.SolarSystem;
            var isEye = scene == OWScene.EyeOfTheUniverse;
            var isPostCredits = scene == OWScene.PostCreditsScene;

            // The GameObject associated with this ModBehaviour is set to persist between scene loads.
            // Some modules need to be restarted on every scene load.
            // This GameObject is for them.
            var nonPersistentObject = new GameObject();

            if (isSolarSystem || isEye) {
                Common.InitGame();
                nonPersistentObject.AddComponent<EffectFixes>();
                nonPersistentObject.AddComponent<PlayerBodyPosition>();
                nonPersistentObject.AddComponent<Dialogue>();
                nonPersistentObject.AddComponent<Hands>();
                nonPersistentObject.AddComponent<FeetMarker>();
                nonPersistentObject.AddComponent<InputPrompts>();
                nonPersistentObject.AddComponent<HelmetHUD>();
                nonPersistentObject.AddComponent<VRTutorial>();
                if (isSolarSystem) {
                    nonPersistentObject.AddComponent<ShipTools>();
                    nonPersistentObject.AddComponent<SolarSystemMap>();
                }
            } else if (isPostCredits) {
                nonPersistentObject.AddComponent<PostCreditsScene>();
            }

            nonPersistentObject.AddComponent<Menus>();
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
