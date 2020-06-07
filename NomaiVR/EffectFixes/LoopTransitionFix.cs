﻿using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class LoopTransitionFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, LoopTransitionFix.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            private static Transform _focus;

            public override void ApplyPatches()
            {
                NomaiVR.Pre<Flashback>("OnTriggerFlashback", typeof(Patch), nameof(PatchTriggerFlashback));
                NomaiVR.Pre<Flashback>("Update", typeof(Patch), nameof(FlashbackUpdate));
                NomaiVR.Pre<Flashback>("UpdateMemoryUplink", typeof(Patch), nameof(PostUpdateMemoryLink));
                NomaiVR.Post<Flashback>("OnTriggerMemoryUplink", typeof(Patch), nameof(PostTriggerMemoryUplink));

                // Prevent flashing on energy death.
                NomaiVR.Post<Flashback>("OnTriggerFlashback", typeof(Patch), nameof(PostTriggerFlashback));
            }

            private static void PostUpdateMemoryLink(
                GameObject ____reverseStreams,
                Transform ____screenTransform,
                ref Vector3 ____origScreenScale,
                ref float ____reverseScreenEndDist,
                ref float ____reverseScreenStartDist
            )
            {
                if (_focus != null)
                {
                    _focus.LookAt(Camera.main.transform, Locator.GetPlayerTransform().up);
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

                var uplinkTrigger = GameObject.FindObjectOfType<MemoryUplinkTrigger>();
                var statue = uplinkTrigger.GetValue<Transform>("_lockOnTransform");
                var eye = statue.Find("Props_NOM_StatueHead/eyelid_mid");
                _focus = new GameObject().transform;
                _focus.SetParent(eye, false);

                var streams = ____reverseStreams.transform;
                LayerHelper.ChangeLayerRecursive(____reverseStreams, LayerMask.NameToLayer("UI"));
                streams.SetParent(_focus, false);
                streams.Rotate(0, 180, 0);
                streams.localScale *= 0.5f;

                var screen = ____screenTransform;
                LayerHelper.ChangeLayerRecursive(screen.gameObject, LayerMask.NameToLayer("UI"));
                screen.SetParent(_focus, false);
                screen.localRotation = Quaternion.identity;
                screen.localScale = scale;
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
