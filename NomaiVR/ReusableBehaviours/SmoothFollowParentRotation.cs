using UnityEngine;

namespace NomaiVR
{
    public class SmoothFollowParentRotation : MonoBehaviour
    {
        [SerializeField] private readonly float step = 10f;

        private Quaternion startLocalRot, lastFrameRot, lastDesiredRot, fromRot;

        private float percent;

        internal void Start()
        {
            startLocalRot = transform.localRotation;
            lastFrameRot = transform.rotation;
        }

        internal void Update()
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