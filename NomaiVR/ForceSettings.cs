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

        static void SetFOV () {
            PlayerData.GetGraphicSettings().fieldOfView = Camera.main.fieldOfView;
            GraphicSettings.s_fovMax = GraphicSettings.s_fovMin = Camera.main.fieldOfView;
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<GraphicSettings>("ApplyAllGraphicSettings", typeof(Patches), nameof(PreApplySettings));
                NomaiVR.Empty<InputRebindableLibrary>("SetKeyBindings");
                NomaiVR.Empty<GraphicSettings>("SetSliderValFOV");
            }

            static void PreApplySettings () {
                SetResolution();
            }
        }
    }
}
