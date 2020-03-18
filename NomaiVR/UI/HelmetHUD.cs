using UnityEngine;

namespace NomaiVR {
    public class HelmetHUD: MonoBehaviour {
        private static Transform _thrusterHUD;
        private static Transform _helmet;

        void Awake () {
            SetupThrusterHUD();

            // Move helmet forward to make it a bit more visible.
            _helmet = FindObjectOfType<HUDHelmetAnimator>().transform;
            _helmet.localPosition += Vector3.forward * 0.2f;

            var surface = GameObject.Find("HUD_CurvedSurface").transform;
            surface.transform.localScale = Vector3.one * 3.28f;
            surface.transform.localPosition = new Vector3(-0.06f, -0.56f, 0.06f);
            surface.gameObject.AddComponent<DebugTransform>();

            var followTarget = _helmet.gameObject.AddComponent<FollowTarget>();
            followTarget.target = Camera.main.transform;
            followTarget.localPosition = Vector3.forward * 0.2f;
            followTarget.rotationSmoothTime = 0.1f;

            _helmet.parent = null;

            var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
            notifications.anchoredPosition = new Vector2(-200, -100);

            var playerHUD = GameObject.Find("PlayerHUD");

            // Fix lock on UI on suit mode.
            var lockOnCanvas = playerHUD.transform.Find("HelmetOffUI/HelmetOffLockOn").GetComponent<Canvas>();
            lockOnCanvas.planeDistance = 10;
        }

        void SetupThrusterHUD () {
            _thrusterHUD = GameObject.Find("HUD_Thrusters").transform;
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Helper.HarmonyHelper.AddPostfix<ThrustAndAttitudeIndicator>("LateUpdate", typeof(Patches), nameof(PatchLateUpdate));
                NomaiVR.Post<HUDCamera>("Awake", typeof(Patches), nameof(PostHUDCameraAwake));
            }

            static void PostHUDCameraAwake (Camera ____camera) {
                // Prevent distortion of helmet HUD.
                ____camera.fieldOfView = 60;
            }

            static void PatchLateUpdate () {
                var rotation = _helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                _thrusterHUD.transform.rotation = rotation;
            }
        }
    }
}
