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
            transform.rotation = RotationSmoothTime > 0
                ? MathHelper.SmoothDamp(transform.rotation, targetRotation, ref rotationVelocity, RotationSmoothTime)
                : targetRotation;

            var targetPosition = Target.TransformPoint(LocalPosition);
            transform.position = PositionSmoothTime > 0
                ? MathHelper.SmoothDamp(transform.position, targetPosition, ref positionVelocity, RotationSmoothTime)
                : targetPosition;
        }
    }
}
