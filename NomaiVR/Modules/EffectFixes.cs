using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class EffectFixes: MonoBehaviour {
        OWCamera _camera;

        void Start () {
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
        }

        void Update () {
            _camera.postProcessingSettings.bloomEnabled = false;
            _camera.postProcessingSettings.chromaticAberrationEnabled = false;
            _camera.postProcessingSettings.colorGradingEnabled = false;
            _camera.postProcessingSettings.phosphenesEnabled = false;
            _camera.postProcessingSettings.vignetteEnabled = false;
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<PlanetaryFogController>("ResetFogSettings", typeof(Patches), nameof(Patches.PatchResetFog));
                NomaiVR.Pre<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), nameof(Patches.PatchUpdateFog));
                NomaiVR.Pre<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), nameof(Patches.PatchOverrideFog));
                NomaiVR.Pre<Flashback>("OnTriggerFlashback", typeof(Patches), nameof(Patches.PatchTriggerFlashback));
                NomaiVR.Pre<Flashback>("Update", typeof(Patches), nameof(Patches.FlashbackUpdate));
                NomaiVR.Post<NomaiRemoteCameraPlatform>("SwitchToRemoteCamera", typeof(Patches), nameof(Patches.SwitchToRemoteCamera));
            }

            static void SwitchToRemoteCamera (NomaiRemoteCameraPlatform ____slavePlatform, Transform ____playerHologram) {
                ____slavePlatform.GetOwnedCamera().transform.parent = ____playerHologram;
                ____playerHologram.Find("Traveller_HEA_Player_v2/Traveller_Mesh_v01:Traveller_Geo/Traveller_Mesh_v01:PlayerSuit_Helmet").gameObject.layer = 0;
                ____playerHologram.Find("Traveller_HEA_Player_v2/player_mesh_noSuit:Traveller_HEA_Player/player_mesh_noSuit:Player_Head").gameObject.layer = 0;
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
