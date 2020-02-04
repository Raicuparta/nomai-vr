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
            NomaiVR.Helper.HarmonyHelper.AddPrefix<Flashback>("OnTriggerFlashback", typeof(Patches), "PatchTriggerFlashback");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<Flashback>("Update", typeof(Patches), "FlashbackUpdate");

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

            static void PatchTriggerFlashback (Flashback __instance, Transform ____maskTransform) {
                if (____maskTransform.parent != __instance.transform) {
                    return;
                }

                var parent = new GameObject().transform;
                parent.position = __instance.transform.position;
                parent.rotation = __instance.transform.rotation;

                foreach (Transform child in __instance.transform) {
                    child.parent = parent;
                    //child.position = Vector3.zero;
                    //child.rotation = Quaternion.identity;
                }
                ____maskTransform.parent = parent;
                //____maskTransform.position = Vector3.zero;
                //____maskTransform.rotation = Quaternion.identity;

                //__instance.transform.position = Vector3.zero;
                //__instance.transform.rotation = Quaternion.identity;
                __instance.GetComponent<Camera>().farClipPlane = 1000;
            }

            static void FlashbackUpdate (Flashback __instance, Transform ____maskTransform) {
                var parent = ____maskTransform.parent;

                parent.position = __instance.transform.position;
            }
        }
    }

}