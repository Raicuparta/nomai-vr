using OWML.ModHelper.Events;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class ForceSettings : NomaiVRModule<ForceSettings.Behaviour, ForceSettings.Behaviour.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        public class Behaviour : MonoBehaviour
        {
            internal void Awake()
            {
                SetResolution();
                SetRefreshRate();
                SetFov();
                ResetInputsToDefault();
            }

            private static void SetResolution()
            {
                var displayResHeight = 720;
                var displayResWidth = 1280;
                var fullScreen = false;

                PlayerPrefs.SetInt("Screenmanager Resolution Width", displayResWidth);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", displayResHeight);
                Screen.SetResolution(displayResWidth, displayResHeight, fullScreen);
            }

            private static void SetFov()
            {
                PlayerData.GetGraphicSettings().fieldOfView = Camera.main.fieldOfView;
                GraphicSettings.s_fovMax = GraphicSettings.s_fovMin = Camera.main.fieldOfView;
            }

            private static void ResetInputsToDefault()
            {
                FindObjectOfType<KeyRebinderManager>().Invoke("OnApplyDefaultsSubmit");
            }

            private static void UpdateActiveController()
            {
                if (OWInput.GetActivePadNumber() != 0)
                {
                    Logs.WriteWarning("Wrong gamepad selected. Resetting to 0");
                    OWInput.SetActiveGamePad(0);
                }
            }

            private static void SetRefreshRate()
            {
                var deviceRefreshRate = SteamVR.instance.hmd_DisplayFrequency;
                var overrideRefreshRate = ModSettings.OverrideRefreshRate;
                var refreshRate = overrideRefreshRate > 0 ? overrideRefreshRate : deviceRefreshRate;
                var fixedTimeStep = 1f / refreshRate;
                var owTime = typeof(OWTime);
                owTime.SetValue("s_fixedTimestep", fixedTimeStep);
                Time.fixedDeltaTime = fixedTimeStep;
            }

            internal void Update()
            {
                UpdateActiveController();
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<GraphicSettings>("ApplyAllGraphicSettings", nameof(PostApplySettings));
                    Empty<InputRebindableLibrary>("SetKeyBindings");
                    Empty<GraphicSettings>("SetSliderValFOV");
                }

                private static void PostApplySettings()
                {
                    SetResolution();
                    SetFov();
                }
            }
        }
    }
}
