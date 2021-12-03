using NomaiVR.ModConfig;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class SmoothFollowCameraRotation : MonoBehaviour
    {
        private Quaternion lastFrameRotation;
        private readonly float speed = 0.5f;
        private bool smoothEnabled = true;

        internal void Start()
        {
            lastFrameRotation = transform.rotation;

            SetSmoothEnabled();

            ModSettings.OnConfigChange += SetSmoothEnabled;
        }

        internal void OnDestroy()
        {
            ModSettings.OnConfigChange -= SetSmoothEnabled;
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
                var difference = Mathf.Abs(Quaternion.Angle(lastFrameRotation, targetRotation));
                var step = speed * Time.unscaledDeltaTime * difference * difference;
                transform.rotation = Quaternion.RotateTowards(lastFrameRotation, targetRotation, step);
            }
            else
            {
                transform.rotation = targetRotation;
            }

            lastFrameRotation = transform.rotation;
        }

        void SetSmoothEnabled()
        {
            smoothEnabled = ModSettings.HudSmoothFollow;
        }
    }
}