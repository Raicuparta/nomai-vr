using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR {
    public class SmoothFoolowParent: MonoBehaviour {
        private float smoothTime = 0.05f;

        private Vector3 positionVelocity;
        private Quaternion rotationVelocity;

        private Vector3 startLocalPos, lastFramePos, lastDesiredPos, fromPos;
        private Quaternion startLocalRot, lastFrameRot, lastDesiredRot, fromRot;

        private float constZ_Pos;

        void Start () {
            startLocalPos = transform.localPosition;
            startLocalRot = transform.localRotation;

            lastFramePos = transform.position;
            lastFrameRot = transform.rotation;

            constZ_Pos = startLocalPos.z;
        }

        void Update () {
            Vector3 newDesiredPos = transform.parent.TransformPoint(startLocalPos);
            Quaternion newDesiredRot = transform.parent.rotation * startLocalRot;

            if (lastDesiredPos != newDesiredPos || lastDesiredRot != newDesiredRot) {
                lastDesiredPos = newDesiredPos;
                lastDesiredRot = newDesiredRot;

                fromPos = lastFramePos;
                fromRot = lastFrameRot;
            }

            lastFramePos = Vector3.SmoothDamp(fromPos, newDesiredPos, ref positionVelocity, smoothTime);
            lastFrameRot = QuaternionHelper.SmoothDamp(fromRot, newDesiredRot, ref rotationVelocity, smoothTime);

            Vector3 newLocalPos = transform.parent.InverseTransformPoint(lastFramePos);
            newLocalPos.z = constZ_Pos;

            transform.localPosition = newLocalPos;
            transform.rotation = lastFrameRot;
        }
    }
}