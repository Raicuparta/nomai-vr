using UnityEngine;

namespace NomaiVR
{
    internal static class CameraHelper
    {
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
    }
}
