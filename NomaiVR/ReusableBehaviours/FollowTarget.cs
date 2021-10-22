using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    internal class FollowTarget : MonoBehaviour
    {
        public Transform target;
        public Vector3 localPosition;
        public Quaternion localRotation = Quaternion.identity;
        public float positionSmoothTime;
        public float rotationSmoothTime;
        private Quaternion rotationVelocity;
        private Vector3 positionVelocity;

        private void Awake()
        {
            Camera.onPreCull += UpdatePosition;
        }

        private void OnDestroy()
        {
            Camera.onPreCull -= UpdatePosition;
        }

        private void UpdatePosition(Camera camera)
        {
            if (!target)
            {
                return;
            }

            var targetRotation = target.rotation * localRotation;
            if (rotationSmoothTime > 0 && Time.timeScale > 0)
            {
                transform.rotation = QuaternionHelper.SmoothDamp(transform.rotation, targetRotation, ref rotationVelocity, rotationSmoothTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            var targetPosition = target.TransformPoint(localPosition);
            if (positionSmoothTime > 0 && Time.timeScale > 0)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, rotationSmoothTime);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }
}
