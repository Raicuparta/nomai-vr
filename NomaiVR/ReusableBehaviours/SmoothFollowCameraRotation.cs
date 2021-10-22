using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class SmoothFollowCameraRotation : MonoBehaviour
    {
        private Quaternion lastFrameRotation;
        private readonly float speed = 0.5f;

        internal void Start()
        {
            lastFrameRotation = transform.rotation;
        }

        internal void LateUpdate()
        {
            if (!Camera.main)
            {
                return;
            }

            var targetRotation = Camera.main.transform.rotation;
            var difference = Mathf.Abs(Quaternion.Angle(lastFrameRotation, targetRotation));

            var step = speed * Time.unscaledDeltaTime * difference * difference;
            transform.rotation = Quaternion.RotateTowards(lastFrameRotation, targetRotation, step);
            lastFrameRotation = transform.rotation;
        }
    }
}