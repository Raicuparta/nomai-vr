using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class EffectFixes : NomaiVRModule<EffectFixes.Behaviour, EffectFixes.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            OWCamera _camera;
            static float _farClipPlane = -1;
            public static int cullingMask = -1;
            static Behaviour _instance;

            void Start()
            {
                _instance = this;

                NomaiVR.Log("Started EffectFixes");

                // Disable underwater effect.
                FindObjectOfType<UnderwaterEffectBubbleController>().gameObject.SetActive(false);

                // Disable water entering and exiting effect.
                var visorEffects = FindObjectOfType<VisorEffectController>();
                visorEffects.SetValue("_waterClearLength", 0);
                visorEffects.SetValue("_waterFadeInLength", 0);

                _camera = Locator.GetPlayerCamera();

                if (LoadManager.GetPreviousScene() == OWScene.TitleScreen && LoadManager.GetCurrentScene() == OWScene.SolarSystem)
                {
                    CloseEyes();
                }
            }

            void Update()
            {
                _camera.postProcessingSettings.chromaticAberrationEnabled = false;
                _camera.postProcessingSettings.vignetteEnabled = false;
            }

            void CloseEyesDelayed()
            {
                Invoke(nameof(CloseEyes), 3);
            }

            void CloseEyes()
            {
                cullingMask = Camera.main.cullingMask;
                _farClipPlane = Camera.main.farClipPlane;
                Camera.main.cullingMask = 1 << LayerMask.NameToLayer("VisibleToPlayer");
                Camera.main.farClipPlane = 5;
                Locator.GetPlayerCamera().postProcessingSettings.eyeMaskEnabled = false;
            }

            void OpenEyes()
            {
                Camera.main.cullingMask = cullingMask;
                Camera.main.farClipPlane = _farClipPlane;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    // Fix for the reprojection stone camera position.
                    NomaiVR.Post<NomaiRemoteCameraPlatform>("SwitchToRemoteCamera", typeof(Patch), nameof(Patch.SwitchToRemoteCamera));

                    NomaiVR.Post<Campfire>("StartFastForwarding", typeof(Patch), nameof(PostStartFastForwarding));

                    var openEyesMethod =
                        typeof(PlayerCameraEffectController)
                        .GetMethod("OpenEyes", new[] { typeof(float), typeof(AnimationCurve) });
                    NomaiVR.Helper.HarmonyHelper.AddPostfix(openEyesMethod, typeof(Patch), nameof(PostOpenEyes));

                    NomaiVR.Post<PlayerCameraEffectController>("CloseEyes", typeof(Patch), nameof(PostCloseEyes));
                }

                static void PostStartFastForwarding()
                {
                    Locator.GetPlayerCamera().enabled = true;
                }

                static void PostOpenEyes()
                {
                    _instance.OpenEyes();
                }

                static void PostCloseEyes()
                {
                    _instance.CloseEyesDelayed();
                }

                static void SwitchToRemoteCamera(NomaiRemoteCameraPlatform ____slavePlatform, Transform ____playerHologram)
                {
                    var camera = ____slavePlatform.GetOwnedCamera().transform;
                    if (camera.parent.name == "Prefab_NOM_RemoteViewer")
                    {
                        var parent = new GameObject().transform;
                        parent.parent = ____playerHologram;
                        parent.localPosition = new Vector3(0, -2.5f, 0);
                        parent.localRotation = Quaternion.identity;
                        ____slavePlatform.GetOwnedCamera().transform.parent = parent;
                        ____playerHologram.Find("Traveller_HEA_Player_v2").gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
