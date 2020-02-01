using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR {
    class LaserPointer: MonoBehaviour {
        static FirstPersonManipulator _manipulator;
        void Awake () {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(Patches), "PatchUpdateInteractVolume");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("OnEntry", typeof(Patches), "InteractZoneEntry");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("OnExit", typeof(Patches), "InteractZoneExit");

            var laser = new GameObject("Laser");
            laser.transform.parent = Hands.RightHand;
            laser.transform.position = Vector3.zero;
            laser.transform.rotation = Quaternion.identity;
            var lineRenderer = Hands.RightHand.gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 3 });
            lineRenderer.endColor = Color.clear;
            lineRenderer.startColor = Color.cyan;
            lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;

            GameObject.FindObjectOfType<FirstPersonManipulator>().enabled = false;
            _manipulator = Hands.RightHand.gameObject.AddComponent<FirstPersonManipulator>();
        }

        internal static class Patches {
            static bool PatchUpdateInteractVolume (
                InteractZone __instance,
                OWCamera ____playerCam,
                float ____viewingWindow,
                ref bool ____focused
            ) {
                float num = 2f * Vector3.Angle(Hands.RightHand.forward, __instance.transform.forward);
                ____focused = (num <= ____viewingWindow);
                var Base = __instance as SingleInteractionVolume;

                var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action) Activator.CreateInstance(typeof(Action), __instance, ftn);

                func();

                return false;
            }

            static bool InteractZoneEntry (GameObject hitObj, InteractZone __instance) {
                if (hitObj.CompareTag("PlayerDetector")) {
                    _manipulator.OnEnterInteractZone(__instance);
                }
                return false;
            }

            static bool InteractZoneExit (GameObject hitObj, InteractZone __instance) {
                if (hitObj.CompareTag("PlayerDetector")) {
                    _manipulator.OnExitInteractZone(__instance);
                }
                return false;
            }
        }

    }
}
