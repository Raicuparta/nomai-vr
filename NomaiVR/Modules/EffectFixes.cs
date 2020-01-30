using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class EffectFixes: MonoBehaviour {
        OWCamera _camera;

        void Start () {
            NomaiVR.Log("Started FogFix");

            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("ResetFogSettings", typeof(Patches), "PatchResetFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), "PatchUpdateFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), "PatchOverrideFog");

            // Make dark bramble lights visible in the fog.
            var fogLightCanvas = GameObject.Find("FogLightCanvas").GetComponent<Canvas>();
            fogLightCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            fogLightCanvas.worldCamera = Locator.GetActiveCamera().mainCamera;
            fogLightCanvas.planeDistance = 100;

            // Disable underwater effect.
            GameObject.FindObjectOfType<UnderwaterEffectBubbleController>().gameObject.SetActive(false);

            // Disable water entering and exiting effect.
            var visorEffects = FindObjectOfType<VisorEffectController>();
            visorEffects.SetValue("_waterClearLength", 0);
            visorEffects.SetValue("_waterFadeInLength", 0);

            _camera = Locator.GetPlayerCamera();
        }

        void Update () {
            _camera.postProcessingSettings.bloomEnabled = false;
            _camera.postProcessingSettings.chromaticAberrationEnabled = false;
            _camera.postProcessingSettings.colorGradingEnabled = false;
            _camera.postProcessingSettings.phosphenesEnabled = false;
            _camera.postProcessingSettings.vignetteEnabled = false;
        }

        internal static class Patches {
            static bool PatchResetFog () {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }
            static bool PatchUpdateFog () {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }
            static bool PatchOverrideFog () {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }
        }
    }

}