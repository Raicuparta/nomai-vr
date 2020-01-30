using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR {
    public class NomaiVR: ModBehaviour {
        public static IModHelper Helper;
        static NomaiVR _instance;
        public static bool MotionControlsEnabled;
        public static bool DebugMode;

        void Start () {
            _instance = this;

            Log("Start Main");

            Helper = ModHelper;

            SteamVR.Initialize();

            // Add all modules here.
            gameObject.AddComponent<Common>();
            if (MotionControlsEnabled) {
                gameObject.AddComponent<ControllerInput>();
            }

            var gameModules = new GameObject();
            gameModules.AddComponent<Menus>();

            Application.runInBackground = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable () {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
            // The GameObject associated with this ModBehaviour is set to persist between scene loads.
            // The following modules need to be restarted on every scene load, so we create a new
            // GameObject for them.

            var isInGame = scene.name == "SolarSystem" || scene.name == "EyeOfTheUniverse";
            var isInTitle = scene.name == "TitleScreen";

            var gameModules = new GameObject();
            gameModules.AddComponent<Menus>().isInGame = isInGame;

            if (isInGame) {
                Common.InitGame();
                gameModules.AddComponent<EffectFixes>();
                gameModules.AddComponent<PlayerBodyPosition>();
                if (MotionControlsEnabled) {
                    gameModules.AddComponent<Hands>();
                }
            } else if (isInTitle) {
                Common.InitPreGame();
            }
        }

        public override void Configure (IModConfig config) {
            DebugMode = config.GetSetting<bool>("debugMode");
            PlayerBodyPosition.MovePlayerWithHead = config.GetSetting<bool>("movePlayerWithHead");
            XRSettings.showDeviceView = config.GetSetting<bool>("showMirrorView");
            MotionControlsEnabled = config.GetSetting<bool>("enableMotionControls");

            if (MotionControlsEnabled) {
                // Prevent application from stealing mouse focus;
                ModHelper.HarmonyHelper.EmptyMethod<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public static void Log (string s) {
            if (DebugMode) {
                _instance.ModHelper.Console.WriteLine("NomaiVR: " + s);
            }
        }
    }
}
