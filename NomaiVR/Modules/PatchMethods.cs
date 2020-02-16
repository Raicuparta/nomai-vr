using System;
using UnityEngine;

namespace NomaiVR {
    static class PatchMethods {
        static void Patch () {
            NomaiVR.Post<ShipBody>("Start", typeof(ShipTools.Patches), "ShipStart");
            NomaiVR.Pre<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(ShipTools.Patches), "PreFindReferenceFrameInLineOfSight");
            NomaiVR.Post<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(ShipTools.Patches), "PostFindReferenceFrameInLineOfSight");
        }
    }
}
