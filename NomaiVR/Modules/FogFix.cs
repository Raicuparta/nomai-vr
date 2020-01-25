using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        private void Start() {
            NomaiVR.Log("Started FogFix");

            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("ResetFogSettings", typeof(Patches), "PatchResetFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), "PatchUpdateFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), "PatchOverrideFog");

            NomaiVR.Helper.Events.Subscribe<CanvasMarkerManager>(OWML.Common.Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(CanvasMarkerManager) && ev == Events.AfterStart) {
                var fogLightCanvas = GameObject.Find("FogLightCanvas").GetComponent<Canvas>();
                fogLightCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                fogLightCanvas.worldCamera = Locator.GetActiveCamera().mainCamera;
                fogLightCanvas.planeDistance = 100;
            }
        }

        internal static class Patches
        {
            static bool PatchResetFog() {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }
            static bool PatchUpdateFog() {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }
            static bool PatchOverrideFog() {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }
        }
    }

}