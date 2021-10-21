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
                UpdateGameLogging();
                ModSettings.OnConfigChange += UpdateGameLogging;
                SetResolution();
                SetFov();
                UnlockMouse();
            }

            internal void OnDestroy()
            {
                ModSettings.OnConfigChange -= UpdateGameLogging;
            }

            private static void UnlockMouse()
            {
                if (!ModSettings.PreventCursorLock) return;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            private static void UpdateGameLogging()
            {
                Debug.unityLogger.logEnabled = ModSettings.DebugMode;
            }

            private static void SetResolution()
            {
                var displayResHeight = 720;
                var displayResWidth = 1280;
                var fullScreen = false;

                PlayerPrefs.SetInt("Screenmanager Resolution Width", displayResWidth);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", displayResHeight);
                QualitySettings.vSyncCount = 0;
                Screen.SetResolution(displayResWidth, displayResHeight, fullScreen);
            }

            private static void SetFov()
            {
                PlayerData.GetGraphicSettings().fieldOfView = Camera.main.fieldOfView;
                GraphicSettings.s_fovMax = GraphicSettings.s_fovMin = Camera.main.fieldOfView;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<GraphicSettings>(nameof(GraphicSettings.ApplyAllGraphicSettings), nameof(PostApplySettings));
                    Postfix<SteamVR_Render>("Update", nameof(UpdateOWTimestep));
                    Empty<GraphicSettings>(nameof(GraphicSettings.SetSliderValFOV));
                    
                    if (ModSettings.PreventCursorLock)
                    {
                        Empty<CursorManager>(nameof(CursorManager.Update));
                    }
                }

                private static void UpdateOWTimestep()
                {
                    OWTime.s_fixedTimestep = Time.fixedDeltaTime;
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
