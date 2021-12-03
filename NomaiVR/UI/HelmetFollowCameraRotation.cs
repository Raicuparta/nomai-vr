using NomaiVR.ModConfig;
using UnityEngine;

namespace NomaiVR.UI
{
    public class HelmetFollowCameraRotation : MonoBehaviour
    {
        private Quaternion lastFrameRotation;
        private const float speed = 0.5f;
        private bool smoothEnabled = true;

        private void Start()
        {
            lastFrameRotation = transform.rotation;

            SetSmoothEnabled();

            ModSettings.OnConfigChange += SetSmoothEnabled;
        }

        private void OnDestroy()
        {
            ModSettings.OnConfigChange -= SetSmoothEnabled;
        }

        private void LateUpdate()
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

        private void SetSmoothEnabled()
        {
            smoothEnabled = ModSettings.HudSmoothFollow;
        }
    }
}