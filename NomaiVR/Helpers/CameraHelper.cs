using UnityEngine;
using UnityEngine.XR;

namespace NomaiVR
{
    internal static class CameraHelper
    {
        private static float _fovFactor = 1.0f;

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
            _fovFactor = fovFactor;
            XRDevice.fovZoomFactor = applyChange ? fovFactor : 1;
        }

        public static float GetFieldOfViewFactor() => _fovFactor;

        public static float GetScaledFieldOfView(Camera camera)
        {
            return camera.fieldOfView / _fovFactor;
        }
    }
}
