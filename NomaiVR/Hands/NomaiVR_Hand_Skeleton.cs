using System.Diagnostics;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.Hands
{
    public class NomaiVRHandSkeleton : SteamVR_Behaviour_Skeleton
    {
        public float BasePoseInfluence { get; set; }
        public Vector3[] BonePositions { get; private set; }
        public Quaternion[] BoneRotations{ get; private set; }
        private bool snapshotCleanRequested;

        public void OnDestroy()
        {
            //Fixes some exceptions when blending just before a scene transition
            StopAllCoroutines();
        }

        public void ClearSnapshot()
        {
            snapshotCleanRequested = true;
        }

        public override void UpdateSkeletonTransforms(Vector3[] bonePositions, Quaternion[] boneRotations)
        {
            Stopwatch stopWatch = new Stopwatch();

            base.UpdateSkeletonTransforms(bonePositions, boneRotations);

            BonePositions = bonePositions;
            BoneRotations = boneRotations; 

            if(snapshotCleanRequested && !isBlending && skeletonBlend > 0 && temporaryRangeOfMotion == null && rangeOfMotion == EVRSkeletalMotionRange.WithoutController)
            {
                blendSnapshot = null;
                snapshotCleanRequested = false;
            }
        }

        public override void SetBonePosition(int boneIndex, Vector3 localPosition)
        {
            if (onlySetRotations == false && BasePoseInfluence > 0)
                localPosition = Vector3.Lerp(bones[boneIndex].localPosition, localPosition, BasePoseInfluence);
            base.SetBonePosition(boneIndex, localPosition);
        }

        public override void SetBoneRotation(int boneIndex, Quaternion localRotation)
        {
            if (BasePoseInfluence > 0)
                localRotation = Quaternion.Lerp(bones[boneIndex].localRotation, localRotation, BasePoseInfluence);
             base.SetBoneRotation(boneIndex, localRotation);
        }
    }
}
