using UnityEngine;

namespace NomaiVR
{
    public class SmoothFollowCameraRotation : MonoBehaviour
    {
        private Quaternion _lastFrameRotation;
        private readonly float _speed = 0.5f;

        internal void Start()
        {
            _lastFrameRotation = transform.rotation;
        }

        internal void LateUpdate()
        {
            var targetRotation = Camera.main.transform.rotation;
            var difference = Mathf.Abs(Quaternion.Angle(_lastFrameRotation, targetRotation));

            var step = _speed * Time.unscaledDeltaTime * difference * difference;
            transform.rotation = Quaternion.RotateTowards(_lastFrameRotation, targetRotation, step);
            _lastFrameRotation = transform.rotation;
        }
    }
}