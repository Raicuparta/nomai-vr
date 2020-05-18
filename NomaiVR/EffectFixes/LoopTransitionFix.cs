using UnityEngine;

namespace NomaiVR
{
    public class LoopTransitionFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, LoopTransitionFix.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                NomaiVR.Pre<Flashback>("OnTriggerFlashback", typeof(Patch), nameof(Patch.PatchTriggerFlashback));
                NomaiVR.Pre<Flashback>("Update", typeof(Patch), nameof(Patch.FlashbackUpdate));

                // Prevent flashing on energy death.
                NomaiVR.Post<Flashback>("OnTriggerFlashback", typeof(Patch), nameof(PostTriggerFlashback));
            }

            static void PatchTriggerFlashback(Flashback __instance, Transform ____maskTransform, Transform ____screenTransform)
            {
                Transform parent;

                if (____screenTransform.parent == __instance.transform)
                {
                    parent = new GameObject().transform;
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

            static void FlashbackUpdate(Flashback __instance, Transform ____maskTransform)
            {
                var parent = ____maskTransform.parent;
                var angle = Quaternion.Angle(parent.rotation, __instance.transform.rotation) * 0.5f;
                parent.rotation = Quaternion.RotateTowards(parent.rotation, __instance.transform.rotation, Time.fixedDeltaTime * angle);
                parent.position = __instance.transform.position;
            }

            static void PostTriggerFlashback(CanvasGroupAnimator ____whiteFadeAnimator)
            {
                ____whiteFadeAnimator.gameObject.SetActive(false);
            }
        }
    }
}
