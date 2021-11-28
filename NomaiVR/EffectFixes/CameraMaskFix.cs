using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class CameraMaskFix : NomaiVRModule<CameraMaskFix.Behaviour, CameraMaskFix.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            public static int DefaultCullingMask;
            private OWCamera camera;
            private static float farClipPlane = -1;
            private static int prePauseCullingMask = -1;
            private static int preSleepCullingMask = -1;
            private static Behaviour instance;
            private bool isPaused;

            internal void Start()
            {
                instance = this;

                SetUpCamera();

                if (SceneHelper.IsPreviousScene(OWScene.TitleScreen) && SceneHelper.IsInSolarSystem())
                {
                    CloseEyes();
                }
            }

            internal void Update()
            {
                UpdatePauseMask();
            }

            private void SetUpCamera()
            {
                camera = Locator.GetPlayerCamera();
                camera.postProcessingSettings.chromaticAberrationEnabled = false;
                camera.postProcessingSettings.vignetteEnabled = false;
                camera.postProcessingSettings.bloom.intensity = 0.15f;
                DefaultCullingMask = camera.cullingMask;
            }

            private void UpdatePauseMask()
            {
                if (InputHelper.IsUIInteractionMode() && !isPaused)
                {
                    SetUpPauseMask();
                }
                if (!InputHelper.IsUIInteractionMode() && isPaused)
                {
                    ResetPauseMask();
                }
            }

            private void SetUpPauseMask()
            {
                isPaused = true;
                prePauseCullingMask = Camera.main.cullingMask;
                Camera.main.cullingMask = LayerMask.GetMask("UI");
            }

            private void ResetPauseMask()
            {
                isPaused = false;
                Camera.main.cullingMask = prePauseCullingMask;
            }

            private void CloseEyesDelayed(float animDuration)
            {
                Invoke(nameof(CloseEyes), animDuration);
            }

            private void CloseEyes()
            {
                CameraHelper.SetFieldOfViewFactor(1, true);
                //We don't want to save the mask when closing the eyes in the dream world
                if (Locator.GetDreamWorldController() == null ||
                    (!Locator.GetDreamWorldController().IsInDream() && !Locator.GetDreamWorldController().IsExitingDream()))
                {
                    preSleepCullingMask = Camera.main.cullingMask;
                    farClipPlane = Camera.main.farClipPlane;
                }
                Camera.main.cullingMask = LayerMask.GetMask("VisibleToPlayer", "UI");
                Camera.main.farClipPlane = 5;
                Locator.GetPlayerCamera().postProcessingSettings.eyeMaskEnabled = false;
            }

            private void OpenEyes()
            {
                Camera.main.cullingMask = preSleepCullingMask;
                Camera.main.farClipPlane = farClipPlane;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<Campfire>(nameof(Campfire.StartFastForwarding), nameof(PostStartFastForwarding));

                    var openEyesMethod =
                        typeof(PlayerCameraEffectController)
                        .GetMethod("OpenEyes", new[] { typeof(float), typeof(AnimationCurve) });
                    Postfix(openEyesMethod, nameof(PostOpenEyes));

                    Postfix<PlayerCameraEffectController>(nameof(PlayerCameraEffectController.CloseEyes), nameof(PostCloseEyes));
                }

                private static void PostStartFastForwarding()
                {
                    Locator.GetPlayerCamera().enabled = true;
                }

                private static void PostOpenEyes()
                {
                    instance.OpenEyes();
                }

                private static void PostCloseEyes(float animDuration)
                {
                    instance.CloseEyesDelayed(animDuration);
                }
            }
        }
    }
}
