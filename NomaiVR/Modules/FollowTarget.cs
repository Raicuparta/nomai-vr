using UnityEngine;

namespace NomaiVR {
    class FollowTarget: MonoBehaviour {
        public Transform target;
        public Vector3 localPosition;
        public Quaternion localRotation = Quaternion.identity;
        public float positionSmoothSpeed = 0;
        public float rotationSmoothSpeed = 0;
        Quaternion rotationVelocity;
        Vector3 positionVelocity;


        void LateUpdate () {
            if (!target) {
                return;
            }

            var targetRotation = target.rotation * localRotation;
            if (rotationSmoothSpeed > 0) {
                transform.rotation = QuaternionHelper.SmoothDamp(transform.rotation, targetRotation, ref rotationVelocity, 0.1f);
            } else {
                transform.rotation = targetRotation;
            }

            var targetPosition = target.TransformPoint(localPosition);
            if (positionSmoothSpeed > 0) {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, 0.1f);
            } else {
                transform.position = targetPosition;
            }

        }
    }
}
