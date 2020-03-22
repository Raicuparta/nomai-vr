using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SmoothFoolowParentRotation: MonoBehaviour {
        [SerializeField] private float step = .1f;

        private Quaternion startLocalRot, lastFrameRot, lastDesiredRot, fromRot;

        private float percent;

        void Start () {
            startLocalRot = transform.localRotation;
            lastFrameRot = transform.rotation;
        }

        void Update () {
            Quaternion newDesiredRot = transform.parent.rotation * startLocalRot;

            if (lastDesiredRot != newDesiredRot) {
                percent = 0;
                lastDesiredRot = newDesiredRot;
                fromRot = lastFrameRot;
            }

            if (percent <= 1) {
                percent += step;
                lastFrameRot = Quaternion.Lerp(fromRot, newDesiredRot, percent);
                transform.rotation = lastFrameRot;
            }
        }
    }
}