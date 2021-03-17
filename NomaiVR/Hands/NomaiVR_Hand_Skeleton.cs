using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.VR;
using Valve.VR.Helpers;

namespace NomaiVR
{
    public class NomaiVR_Hand_Skeleton : SteamVR_Behaviour_Skeleton
    {
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

        public override void UpdateSkeletonTransforms()
        {
            base.UpdateSkeletonTransforms();

            if(_snapshotCleanRequested && skeletonBlend > 0)
            {
                blendSnapshot = null;
                _snapshotCleanRequested = false;
            }
        }
    }
}
