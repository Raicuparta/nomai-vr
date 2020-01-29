using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR {
    public class NomaiVR: ModBehaviour {
        public static IModHelper Helper;
        static NomaiVR _instance;
        bool _motionControls;
        public static bool DebugMode;

        public static void Log (string s) {
            if (DebugMode) {
                _instance.ModHelper.Console.WriteLine("NomaiVR: " + s);
            }
        }

        void Start () {
            _instance = this;

            Log("Start Main");

            Helper = ModHelper;

            SteamVR.Initialize();

            // Add all modules here.
            gameObject.AddComponent<Common>();
            gameObject.AddComponent<Menus>();
            gameObject.AddComponent<EffectFixes>();
            gameObject.AddComponent<PlayerBodyPosition>();
            if (_motionControls) {
                gameObject.AddComponent<ControllerInput>();
                gameObject.AddComponent<Hands>();
            }
        }

        public override void Configure (IModConfig config) {
            _motionControls = config.GetSetting<bool>("enableMotionControls");
            DebugMode = config.GetSetting<bool>("debugMode");
            PlayerBodyPosition.MovePlayerWithHead = config.GetSetting<bool>("movePlayerWithHead");
            XRSettings.showDeviceView = config.GetSetting<bool>("showMirrorView");

            Application.runInBackground = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
