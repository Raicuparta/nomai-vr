using UnityEngine;

namespace NomaiVR
{
    internal class PeepholeCameraFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, PeepholeCameraFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Postfix<Peephole>(nameof(Peephole.SwitchToPeepholeCamera), nameof(SwitchToPeepholeCamera));
            }

            private static void SwitchToPeepholeCamera(Peephole __instance)
            {
                if (__instance._peepholeCamera.transform.parent.name != "VROffsetFixer")
                {
                    Transform peepHoleParent = __instance._peepholeCamera.transform.parent;
                    var playerTransform = Locator.GetPlayerTransform();
                    var playerHeight = playerTransform.InverseTransformVector(Locator.GetPlayerCamera().transform.position - playerTransform.position);
                    var parent = new GameObject("VROffsetFixer").transform;
                    parent.parent = peepHoleParent;
                    parent.localPosition = __instance._peepholeCamera.transform.localPosition - Vector3.up*playerHeight.y + Vector3.forward*0.3f;
                    parent.localRotation = Quaternion.identity;
                    __instance._peepholeCamera.transform.parent = parent;
                }
            }
        }
    }
}
