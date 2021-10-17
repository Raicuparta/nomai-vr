using UnityEngine;

namespace NomaiVR
{
    public class SmoothFollowCameraRotation : MonoBehaviour
    {
        private Quaternion _lastFrameRotation;
        private readonly float _speed = 0.5f;
        private bool smoothEnabled = true;

        internal void Start()
        {
            _lastFrameRotation = transform.rotation;
        }

        internal void LateUpdate()
        {
            if (!Camera.main)
            {
                return;
            }

            var targetRotation = Camera.main.transform.rotation;

            if (smoothEnabled)
            {
                var difference = Mathf.Abs(Quaternion.Angle(_lastFrameRotation, targetRotation));
                var step = _speed * Time.unscaledDeltaTime * difference * difference;
                transform.rotation = Quaternion.RotateTowards(_lastFrameRotation, targetRotation, step);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            _lastFrameRotation = transform.rotation;
        }
    }
}