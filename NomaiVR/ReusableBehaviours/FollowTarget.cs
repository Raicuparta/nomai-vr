using UnityEngine;

namespace NomaiVR {
    class FollowTarget: MonoBehaviour {
        public Transform target;
        public Vector3 localPosition;
        public Quaternion localRotation = Quaternion.identity;
        public float positionSmoothTime = 0;
        public float rotationSmoothTime = 0;
        Quaternion rotationVelocity;
        Vector3 positionVelocity;


        void Update () {
            if (!target) {
                return;
            }

            var targetRotation = target.rotation * localRotation;
            if (rotationSmoothTime > 0 && Time.timeScale > 0) {
                transform.rotation = QuaternionHelper.SmoothDamp(transform.rotation, targetRotation, ref rotationVelocity, rotationSmoothTime);
            } else {
                transform.rotation = targetRotation;
            }

            var targetPosition = target.TransformPoint(localPosition);
            if (positionSmoothTime > 0 && Time.timeScale > 0) {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, rotationSmoothTime);
            } else {
                transform.position = targetPosition;
            }

        }
    }
}
