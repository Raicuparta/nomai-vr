using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class LoopTransitionFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, LoopTransitionFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            private static Transform focus;

            public override void ApplyPatches()
            {
                Prefix<Flashback>(nameof(Flashback.OnTriggerFlashback), nameof(PatchTriggerFlashback));
                Prefix<Flashback>(nameof(Flashback.Update), nameof(FlashbackUpdate));
                Prefix<Flashback>(nameof(Flashback.UpdateMemoryUplink), nameof(PostUpdateMemoryLink));
                Postfix<Flashback>(nameof(Flashback.OnTriggerMemoryUplink), nameof(PostTriggerMemoryUplink));

                // Prevent flashing on energy death.
                Postfix<Flashback>(nameof(Flashback.OnTriggerFlashback), nameof(PostTriggerFlashback));

                // Fix loop picture scale.
                Postfix<Flashback>(nameof(Flashback.Start), nameof(PostFlashbackRecorderAwake));
            }

            private static void PostFlashbackRecorderAwake(Transform ____screenTransform, ref Vector3 ____origScreenScale)
            {
                var scale = ____screenTransform.localScale;
                ____origScreenScale = ____screenTransform.localScale = new Vector3(scale.x * 0.75f, scale.y * 1.5f, scale.z);
            }

            private static void PostUpdateMemoryLink()
            {
                if (focus != null)
                {
                    focus.LookAt(Camera.main.transform, Locator.GetPlayerTransform().up);
                }
            }

            private static void PostTriggerMemoryUplink(
                GameObject ____reverseStreams,
                Transform ____screenTransform,
                ref Vector3 ____origScreenScale,
                ref float ____reverseScreenEndDist,
                ref float ____reverseScreenStartDist
            )
            {
                ____reverseScreenEndDist = 0.2f;
                ____reverseScreenStartDist = 2.5f;
                ____origScreenScale *= 0.5f;
                var scale = ____origScreenScale;

                var uplinkTrigger = Object.FindObjectOfType<MemoryUplinkTrigger>();
                var statue = uplinkTrigger._lockOnTransform;
                var eye = statue.Find("Props_NOM_StatueHead/eyelid_mid");
                focus = new GameObject("VrMemoryUplinkFocus").transform;
                focus.SetParent(eye, false);

                var streams = ____reverseStreams.transform;
                LayerHelper.ChangeLayerRecursive(____reverseStreams, LayerMask.NameToLayer("UI"));
                streams.SetParent(focus, false);
                streams.Rotate(0, 180, 0);
                streams.localScale *= 0.5f;

                var screen = ____screenTransform;
                LayerHelper.ChangeLayerRecursive(screen.gameObject, LayerMask.NameToLayer("UI"));
                screen.SetParent(focus, false);
                screen.localRotation = Quaternion.identity;
                screen.localScale = scale;
            }

            private static void PatchTriggerFlashback(Flashback __instance, Transform ____maskTransform, Transform ____screenTransform)
            {
                Transform parent;
                CameraHelper.SetFieldOfViewFactor(1, true);
                CameraHelper.ActivateCameraTracking(__instance._flashbackCamera.mainCamera, true);

                if (____screenTransform.parent == __instance.transform)
                {
                    parent = new GameObject("VrFlashbackWrapper").transform;
                    parent.position = __instance.transform.position;
                    parent.rotation = __instance.transform.rotation;
                    foreach (Transform child in __instance.transform)
                    {
                        child.parent = parent;
                    }
                }
                else
                {
                    parent = ____screenTransform.parent;
                }


                parent.position = __instance.transform.position;
                parent.rotation = __instance.transform.rotation;

                ____maskTransform.parent = parent;
            }

            private static void FlashbackUpdate(Flashback __instance, Transform ____maskTransform)
            {
                var parent = ____maskTransform.parent;
                var angle = Quaternion.Angle(parent.rotation, __instance.transform.rotation) * 0.5f;
                parent.rotation = Quaternion.RotateTowards(parent.rotation, __instance.transform.rotation, Time.fixedDeltaTime * angle);
                parent.position = __instance.transform.position;
            }

            private static void PostTriggerFlashback(CanvasGroupAnimator ____whiteFadeAnimator)
            {
                ____whiteFadeAnimator.gameObject.SetActive(false);
            }
        }
    }
}
