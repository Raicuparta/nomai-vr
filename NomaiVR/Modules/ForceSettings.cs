using UnityEngine;

namespace NomaiVR {
    class ForceSettings: MonoBehaviour {
        void Awake () {
            SetResolution();
        }

        static void SetResolution () {
            var displayResHeight = 720;
            var displayResWidth = 1280;
            var fullScreen = false;

            PlayerPrefs.SetInt("Screenmanager Resolution Width", displayResWidth);
            PlayerPrefs.SetInt("Screenmanager Resolution Height", displayResHeight);
            Screen.SetResolution(displayResWidth, displayResHeight, fullScreen);
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<GraphicSettings>("ApplyAllGraphicSettings", typeof(Patches), nameof(PreApplySettings));
            }

            static void PreApplySettings () {
                SetResolution();
            }
        }
    }
}
