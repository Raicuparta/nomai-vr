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

        public void ClearSnapshot()
        {
            _snapshotCleanRequested = true;
        }

        public override void UpdateSkeletonTransforms()
        {
            base.UpdateSkeletonTransforms();

            if(_snapshotCleanRequested)
            {
                blendSnapshot = null;
                _snapshotCleanRequested = false;
            }
        }
    }
}
