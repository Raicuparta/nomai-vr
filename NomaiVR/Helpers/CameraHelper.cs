using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace NomaiVR.Helpers
{
    internal static class CameraHelper
    {
        private static float fovFactor = 1.0f;

        public static bool IsOnScreen(Vector3 position)
        {
            var camera = Camera.current;
            if (!camera)
            {
                return false;
            }
            var screenPoint = Camera.current.WorldToViewportPoint(position);
            return screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
        }

        public static void SetFieldOfViewFactor(float fovFactor, bool applyChange = false)
        {
            CameraHelper.fovFactor = fovFactor;
            XRDevice.fovZoomFactor = applyChange ? fovFactor : 1;
        }

        public static float GetFieldOfViewFactor() => fovFactor;

        public static float GetScaledFieldOfView(Camera camera)
        {
            return camera.fieldOfView / fovFactor;
        }


        public static void ActivateCameraTracking(Camera camera, bool relativeTransform = false)
        {
            var hmdTracking = camera.gameObject.GetAddComponent<TrackedPoseDriver>();
            hmdTracking.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Head);
            hmdTracking.updateType = TrackedPoseDriver.UpdateType.BeforeRender;
            hmdTracking.UseRelativeTransform = relativeTransform;
        }
    }
}
