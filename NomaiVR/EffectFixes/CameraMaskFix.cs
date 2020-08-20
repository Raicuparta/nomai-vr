using UnityEngine;

namespace NomaiVR
{
    internal class CameraMaskFix : NomaiVRModule<CameraMaskFix.Behaviour, CameraMaskFix.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            public static int DefaultCullingMask;
            private OWCamera _camera;
            private static float _farClipPlane = -1;
            private static int _prePauseCullingMask = -1;
            private static int _preSleepCullingMask = -1;
            private static Behaviour _instance;
            private bool _isPaused;

            internal void Start()
            {
                _instance = this;

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
                _camera = Locator.GetPlayerCamera();
                _camera.postProcessingSettings.chromaticAberrationEnabled = false;
                _camera.postProcessingSettings.vignetteEnabled = false;
                _camera.postProcessingSettings.bloom.intensity = 0.15f;
                DefaultCullingMask = _camera.cullingMask;
            }

            private void UpdatePauseMask()
            {
                if (InputHelper.IsUIInteractionMode() && !_isPaused)
                {
                    SetUpPauseMask();
                }
                if (!InputHelper.IsUIInteractionMode() && _isPaused)
                {
                    ResetPauseMask();
                }
            }

            private void SetUpPauseMask()
            {
                _isPaused = true;
                _prePauseCullingMask = Camera.main.cullingMask;
                Camera.main.cullingMask = LayerMask.GetMask("UI");
            }

            private void ResetPauseMask()
            {
                _isPaused = false;
                Camera.main.cullingMask = _prePauseCullingMask;
            }

            private void CloseEyesDelayed()
            {
                Invoke(nameof(CloseEyes), 3);
            }

            private void CloseEyes()
            {
                _preSleepCullingMask = Camera.main.cullingMask;
                _farClipPlane = Camera.main.farClipPlane;
                Camera.main.cullingMask = LayerMask.GetMask("VisibleToPlayer", "UI");
                Camera.main.farClipPlane = 5;
                Locator.GetPlayerCamera().postProcessingSettings.eyeMaskEnabled = false;
            }

            private void OpenEyes()
            {
                Camera.main.cullingMask = _preSleepCullingMask;
                Camera.main.farClipPlane = _farClipPlane;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<Campfire>("StartFastForwarding", nameof(PostStartFastForwarding));

                    var openEyesMethod =
                        typeof(PlayerCameraEffectController)
                        .GetMethod("OpenEyes", new[] { typeof(float), typeof(AnimationCurve) });
                    Postfix(openEyesMethod, nameof(PostOpenEyes));

                    Postfix<PlayerCameraEffectController>("CloseEyes", nameof(PostCloseEyes));
                }

                private static void PostStartFastForwarding()
                {
                    Locator.GetPlayerCamera().enabled = true;
                }

                private static void PostOpenEyes()
                {
                    _instance.OpenEyes();
                }

                private static void PostCloseEyes()
                {
                    _instance.CloseEyesDelayed();
                }
            }
        }
    }
}
