using OWML.ModHelper.Events;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class ForceSettings : NomaiVRModule<ForceSettings.Behaviour, ForceSettings.Patch>
    {
        protected override bool isPersistent => true;
        protected override OWScene[] scenes => TitleScene;

        public class Behaviour : MonoBehaviour
        {
            void Awake()
            {
                SetResolution();
                SetRefreshRate();
            }

            void SetRefreshRate()
            {
                var deviceRefreshRate = SteamVR.instance.hmd_DisplayFrequency;
                var overrideRefreshRate = NomaiVR.RefreshRate;
                var refreshRate = overrideRefreshRate > 0 ? overrideRefreshRate : deviceRefreshRate;
                var fixedTimeStep = 1f / refreshRate;
                var owTime = typeof(OWTime);
                owTime.SetValue("s_fixedTimestep", fixedTimeStep);
                Time.fixedDeltaTime = fixedTimeStep;
            }

            public static void SetResolution()
            {
                var displayResHeight = 720;
                var displayResWidth = 1280;
                var fullScreen = false;

                PlayerPrefs.SetInt("Screenmanager Resolution Width", displayResWidth);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", displayResHeight);
                Screen.SetResolution(displayResWidth, displayResHeight, fullScreen);
            }

            static void SetFOV()
            {
                PlayerData.GetGraphicSettings().fieldOfView = Camera.main.fieldOfView;
                GraphicSettings.s_fovMax = GraphicSettings.s_fovMin = Camera.main.fieldOfView;
            }
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                NomaiVR.Post<GraphicSettings>("ApplyAllGraphicSettings", typeof(Patch), nameof(PreApplySettings));
                NomaiVR.Empty<InputRebindableLibrary>("SetKeyBindings");
                NomaiVR.Empty<GraphicSettings>("SetSliderValFOV");
            }

            static void PreApplySettings()
            {
                Behaviour.SetResolution();
            }
        }
    }
}
