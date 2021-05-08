using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;
using Valve.VR.Helpers;

namespace NomaiVR
{
    public class NomaiVR_Hand_Skeleton : SteamVR_Behaviour_Skeleton
    {
        public float basePoseInfluence { get; set; } = 0f;
        private bool _snapshotCleanRequested = false;

        public void OnDestroy()
        {
            //Fixes some exceptions when blending just before a scene transition
            StopAllCoroutines();
        }

        public void ClearSnapshot()
        {
            _snapshotCleanRequested = true;
        }

        public override void UpdateSkeletonTransforms(Vector3[] bonePositions, Quaternion[] boneRotations)
        {
            base.UpdateSkeletonTransforms(bonePositions, boneRotations);

            if(_snapshotCleanRequested && !isBlending && skeletonBlend > 0 && temporaryRangeOfMotion == null && rangeOfMotion == EVRSkeletalMotionRange.WithoutController)
            {
                blendSnapshot = null;
                _snapshotCleanRequested = false;
            }
        }

        public override void SetBonePosition(int boneIndex, Vector3 localPosition)
        {
            if (onlySetRotations == false && basePoseInfluence > 0)
                localPosition = Vector3.Lerp(bones[boneIndex].localPosition, localPosition, basePoseInfluence);
            base.SetBonePosition(boneIndex, localPosition);
        }

        public override void SetBoneRotation(int boneIndex, Quaternion localRotation)
        {
            if (basePoseInfluence > 0)
                localRotation = Quaternion.Lerp(bones[boneIndex].localRotation, localRotation, basePoseInfluence);
             base.SetBoneRotation(boneIndex, localRotation);
        }
    }
}
