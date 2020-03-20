using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR {
    public class SmoothFoolowParentRotation: MonoBehaviour {
        private float smoothTime = 0.05f;

        private Quaternion rotationVelocity;
        private Quaternion startLocalRot, lastFrameRot, lastDesiredRot, fromRot;

        void Start () {
            startLocalRot = transform.localRotation;
            lastFrameRot = transform.rotation;
        }

        void Update () {
            Quaternion newDesiredRot = transform.parent.rotation * startLocalRot;

            if (lastDesiredRot != newDesiredRot) {
                lastDesiredRot = newDesiredRot;

                fromRot = lastFrameRot;
            }

            lastFrameRot = QuaternionHelper.SmoothDamp(fromRot, newDesiredRot, ref rotationVelocity, smoothTime);

            transform.rotation = lastFrameRot;
        }
    }
}