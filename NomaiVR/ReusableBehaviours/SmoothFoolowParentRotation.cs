using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR {
    public class SmoothFoolowParentRotation: MonoBehaviour {
        private float smoothTime = 0.08f;

        private Quaternion rotationVelocity;
        private Quaternion startLocalRot, lastFrameRot, lastDesiredRot, fromRot;

        void Start () {
            startLocalRot = transform.localRotation;
            lastFrameRot = transform.rotation;
        }

        void LateUpdate () {
            Quaternion newDesiredRot = transform.parent.rotation * startLocalRot;

            if (lastDesiredRot != newDesiredRot) {
                lastDesiredRot = newDesiredRot;

                fromRot = lastFrameRot;
            }

            if (Time.timeScale != 1) {
                lastFrameRot = newDesiredRot;
                rotationVelocity = Quaternion.identity;
                transform.localRotation = startLocalRot;
            } else {
                lastFrameRot = QuaternionHelper.SmoothDamp(fromRot, newDesiredRot, ref rotationVelocity, smoothTime);
                transform.rotation = lastFrameRot;
            }
        }
    }
}