using UnityEngine;

namespace NomaiVR
{
    public class SmoothFollowParentRotation : MonoBehaviour
    {
        private Quaternion _lastFrameRotation;
        private float _speed = 15f;

        internal void Start()
        {
            _lastFrameRotation = transform.rotation;
        }

        internal void LateUpdate()
        {
            var targetRotation = Camera.main.transform.rotation;
            var difference = Quaternion.Angle(_lastFrameRotation, targetRotation);

            var step = _speed * Time.unscaledDeltaTime * difference;
            transform.rotation = Quaternion.RotateTowards(_lastFrameRotation, targetRotation, step);
            _lastFrameRotation = transform.rotation;
        }
    }
}