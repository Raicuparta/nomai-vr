using System;
using UnityEngine;

namespace NomaiVR {
    static class PatchMethods {
        static void Patch () {
            NomaiVR.Helper.HarmonyHelper.AddPostfix<ShipBody>("Start", typeof(ShipTools.Patches), "ShipStart");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(ShipTools.Patches), "PreFindReferenceFrameInLineOfSight");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(ShipTools.Patches), "PostFindReferenceFrameInLineOfSight");
        }
    }
}
