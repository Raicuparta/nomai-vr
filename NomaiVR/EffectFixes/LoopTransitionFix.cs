using OWML.ModHelper.Events;
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
                NomaiVR.Post<Flashback>("OnTriggerMemoryUplink", typeof(Patch), nameof(PostTriggerMemoryUplink));
            }

            private static void PostTriggerMemoryUplink(
                GameObject ____reverseStreams,
                Transform ____screenTransform,
                ref Vector3 ____origScreenScale,
                ref float ____reverseScreenEndDist,
                ref float ____reverseScreenStartDist
            )
            {
                ____reverseScreenEndDist = 0f;
                ____reverseScreenStartDist = 3f;
                ____origScreenScale *= 0.5f;
                var scale = ____origScreenScale;

                var uplinkTrigger = GameObject.FindObjectOfType<MemoryUplinkTrigger>();
                var statue = uplinkTrigger.GetValue<Transform>("_lockOnTransform");
                var eye = statue.Find("Props_NOM_StatueHead/eyelid_mid");
                var focus = new GameObject().transform;
                focus.SetParent(eye, false);
                focus.LookAt(Camera.main.transform);

                var streams = ____reverseStreams.transform;
                LayerHelper.ChangeLayerRecursive(____reverseStreams, LayerMask.NameToLayer("UI"));
                streams.SetParent(focus, false);
                streams.LookAt(2 * streams.position - Camera.main.transform.position);
                streams.localPosition = -Vector3.forward;
                streams.localScale *= 0.3f;

                var screen = ____screenTransform;
                LayerHelper.ChangeLayerRecursive(screen.gameObject, LayerMask.NameToLayer("UI"));
                screen.SetParent(focus, false);
                screen.localRotation = Quaternion.identity;
                screen.LookAt(Camera.main.transform.position, Locator.GetPlayerTransform().up);
                //var scale = screen.localScale * 0.5f;
                //scale.z *= -1;
                screen.localScale = scale;
                screen.localPosition = Camera.main.transform.position;
            }

            private static void PatchTriggerFlashback(Flashback __instance, Transform ____maskTransform, Transform ____screenTransform)
            {
                NomaiVR.Log("Trigger flashback");
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
