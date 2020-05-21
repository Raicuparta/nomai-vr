using UnityEngine;

namespace NomaiVR
{
    public class SmoothFoolowParentRotation : MonoBehaviour
    {
        [SerializeField] private float step = 10f;

        private Quaternion startLocalRot, lastFrameRot, lastDesiredRot, fromRot;

        private float percent;

        private void Start()
        {
            startLocalRot = transform.localRotation;
            lastFrameRot = transform.rotation;
        }

        private void Update()
        {
            var newDesiredRot = transform.parent.rotation * startLocalRot;

            if (lastDesiredRot != newDesiredRot)
            {
                percent = 0;
                lastDesiredRot = newDesiredRot;
                fromRot = lastFrameRot;
            }

            if (percent <= 1)
            {
                percent += step * Time.unscaledDeltaTime;
                lastFrameRot = Quaternion.Lerp(fromRot, newDesiredRot, percent);
                transform.rotation = lastFrameRot;
            }
        }
    }
}