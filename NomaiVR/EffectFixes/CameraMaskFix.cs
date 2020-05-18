using UnityEngine;

namespace NomaiVR
{
    public class CameraMaskFix : NomaiVRModule<CameraMaskFix.Behaviour, CameraMaskFix.Behaviour.Patch>
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
            }
        }
    }
}
