
using UnityEngine;

namespace NomaiVR
{
    internal class ShadowsFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ShadowsFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<CSMTextureCacher>("OnAnyCameraPostRender", nameof(Patch.PreOnAnyCameraPostRender));
            }

            private static bool PreOnAnyCameraPostRender(Camera camera)
            {
                return !camera.stereoEnabled || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }
        }
    }
}
