using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR {
    public class EffectFixes: MonoBehaviour {
        OWCamera _camera;
        static float _farClipPlane = -1;
        static int _cullingMask;
        static EffectFixes _instance;

        void Start () {
            _instance = this;

            NomaiVR.Log("Started FogFix");

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

            if (SceneManager.GetActiveScene().name == "SolarSystem") {
                CloseEyes();
            }
        }

        void Update () {
            _camera.postProcessingSettings.chromaticAberrationEnabled = false;
            _camera.postProcessingSettings.vignetteEnabled = false;
        }

        void CloseEyesDelayed () {
            Invoke(nameof(CloseEyes), 3);
        }

        void CloseEyes () {
            _cullingMask = Camera.main.cullingMask;
            _farClipPlane = Camera.main.farClipPlane;
            Camera.main.cullingMask = 1 << LayerMask.NameToLayer("VisibleToPlayer");
            Camera.main.farClipPlane = 5;
            Locator.GetPlayerCamera().postProcessingSettings.eyeMaskEnabled = false;
        }

        void OpenEyes () {
            Camera.main.cullingMask = _cullingMask;
            Camera.main.farClipPlane = _farClipPlane;
        }

        internal static class Patches {
            public static void Patch () {
                // Fixes for fog stereo problems.
                NomaiVR.Pre<PlanetaryFogController>("ResetFogSettings", typeof(Patches), nameof(Patches.PatchResetFog));
                NomaiVR.Pre<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), nameof(Patches.PatchUpdateFog));
                NomaiVR.Pre<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), nameof(Patches.PatchOverrideFog));

                // Improvements for the "loop reset" effect.
                NomaiVR.Pre<Flashback>("OnTriggerFlashback", typeof(Patches), nameof(Patches.PatchTriggerFlashback));
                NomaiVR.Pre<Flashback>("Update", typeof(Patches), nameof(Patches.FlashbackUpdate));

                // Fix for the reprojection stone camera position.
                NomaiVR.Post<NomaiRemoteCameraPlatform>("SwitchToRemoteCamera", typeof(Patches), nameof(Patches.SwitchToRemoteCamera));

                // Prevent flashing on energy death.
                NomaiVR.Post<Flashback>("OnTriggerFlashback", typeof(Patches), nameof(PostTriggerFlashback));

                NomaiVR.Post<Campfire>("StartFastForwarding", typeof(Patches), nameof(PostStartFastForwarding));

                var openEyesMethod =
                    typeof(PlayerCameraEffectController)
                    .GetMethod("OpenEyes", new[] { typeof(float), typeof(AnimationCurve) });
                NomaiVR.Helper.HarmonyHelper.AddPostfix(openEyesMethod, typeof(Patches), nameof(PostOpenEyes));

                NomaiVR.Post<PlayerCameraEffectController>("CloseEyes", typeof(Patches), nameof(PostCloseEyes));

            }

            static void PostStartFastForwarding () {
                Locator.GetPlayerCamera().enabled = true;
            }

            static void PostOpenEyes () {
                _instance.OpenEyes();
            }

            static void PostCloseEyes () {
                _instance.CloseEyesDelayed();
            }

            static void PostTriggerFlashback (CanvasGroupAnimator ____whiteFadeAnimator) {
                ____whiteFadeAnimator.gameObject.SetActive(false);
            }

            static void SwitchToRemoteCamera (NomaiRemoteCameraPlatform ____slavePlatform, Transform ____playerHologram) {
                var camera = ____slavePlatform.GetOwnedCamera().transform;
                if (camera.parent.name == "Prefab_NOM_RemoteViewer") {
                    var parent = new GameObject().transform;
                    parent.parent = ____playerHologram;
                    parent.localPosition = new Vector3(0, -2.5f, 0);
                    parent.localRotation = Quaternion.identity;
                    ____slavePlatform.GetOwnedCamera().transform.parent = parent;
                    ____playerHologram.Find("Traveller_HEA_Player_v2").gameObject.SetActive(false);
                }
            }

            static bool PatchResetFog () {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            static bool PatchUpdateFog () {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }

            static bool PatchOverrideFog () {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }

            static void PatchTriggerFlashback (Flashback __instance, Transform ____maskTransform, Transform ____screenTransform) {
                Transform parent;

                if (____screenTransform.parent == __instance.transform) {
                    parent = new GameObject().transform;
                    parent.position = __instance.transform.position;
                    parent.rotation = __instance.transform.rotation;
                    foreach (Transform child in __instance.transform) {
                        child.parent = parent;
                    }
                } else {
                    parent = ____screenTransform.parent;
                }


                parent.position = __instance.transform.position;
                parent.rotation = __instance.transform.rotation;

                ____maskTransform.parent = parent;
            }

            static void FlashbackUpdate (Flashback __instance, Transform ____maskTransform) {
                var parent = ____maskTransform.parent;
                var angle = Quaternion.Angle(parent.rotation, __instance.transform.rotation) * 0.5f;
                parent.rotation = Quaternion.RotateTowards(parent.rotation, __instance.transform.rotation, Time.fixedDeltaTime * angle);
                parent.position = __instance.transform.position;
            }
        }
    }

}
