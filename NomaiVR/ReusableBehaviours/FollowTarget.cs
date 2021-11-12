using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class FollowTarget : MonoBehaviour
    {
        public enum UpdateType
        {
            PreCull,
            LateUpdate,
        }
        
        public Transform Target;
        public Vector3 LocalPosition;
        public Quaternion LocalRotation = Quaternion.identity;
        public float PositionSmoothTime;
        public float RotationSmoothTime;
        public UpdateType updateType = UpdateType.LateUpdate;
        private Quaternion rotationVelocity;
        private Vector3 positionVelocity;
        private Camera mainCamera;

        private void Start()
        {
            if (updateType == UpdateType.PreCull) SetUpPreCull();
        }

        private void OnDestroy()
        {
            if (updateType == UpdateType.PreCull) CleanUpPreCull();
        }

        private void LateUpdate()
        {
            if (updateType != UpdateType.LateUpdate) return;
            UpdateTransform();
        }

        private void HandleCameraPrecull(Camera camera)
        {
            if (!Target || camera != mainCamera) return;

            UpdateTransform();
        }

        private void SetUpPreCull()
        {
            mainCamera = Camera.main;
            Camera.onPreCull += HandleCameraPrecull;
        }

        private void CleanUpPreCull()
        {
            Camera.onPreCull -= HandleCameraPrecull;
        }

        private void UpdateTransform()
        {
            var targetRotation = Target.rotation * LocalRotation;
            if (RotationSmoothTime > 0 && Time.timeScale > 0)
            {
                transform.rotation = QuaternionHelper.SmoothDamp(transform.rotation, targetRotation, ref rotationVelocity, RotationSmoothTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            var targetPosition = Target.TransformPoint(LocalPosition);
            if (PositionSmoothTime > 0 && Time.timeScale > 0)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, RotationSmoothTime);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }
}
