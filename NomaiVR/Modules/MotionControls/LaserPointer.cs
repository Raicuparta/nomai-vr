using System;
using UnityEngine;

namespace NomaiVR {
    class LaserPointer: MonoBehaviour {
        static FirstPersonManipulator _manipulator;
        static Transform _laser;

        void Awake () {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(Patches), "PatchUpdateInteractVolume");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("OnEntry", typeof(Patches), "InteractZoneEntry");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("OnExit", typeof(Patches), "InteractZoneExit");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<ToolModeSwapper>("Update", typeof(Patches), "ToolModeUpdate");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<ItemTool>("UpdateIsDroppable", typeof(Patches), "PreUpdateIsDroppable");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<ItemTool>("UpdateIsDroppable", typeof(Patches), "PostUpdateIsDroppable");

            _laser = new GameObject("Laser").transform;
            _laser.transform.parent = Hands.RightHand;
            _laser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
            _laser.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            var lineRenderer = _laser.gameObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 3 });
            lineRenderer.endColor = new Color(1, 1, 1, 0.5f);
            lineRenderer.startColor = Color.clear;
            lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.01f;

            _laser.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRender;

            GameObject.FindObjectOfType<FirstPersonManipulator>().enabled = false;
            _manipulator = _laser.gameObject.AddComponent<FirstPersonManipulator>();

            DisableReticule();
        }

        bool ShouldRender () {
            return Common.ToolSwapper.IsInToolMode(ToolMode.None) || Common.ToolSwapper.IsInToolMode(ToolMode.Item);
        }

        void DisableReticule () {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var rootObject in rootObjects) {
                if (rootObject.name == "Reticule") {
                    rootObject.SetActive(false);
                    return;
                }
            }
        }

        static class Patches {
            static bool PatchUpdateInteractVolume (
                InteractZone __instance,
                OWCamera ____playerCam,
                float ____viewingWindow,
                ref bool ____focused
            ) {
                float num = 2f * Vector3.Angle(_laser.forward, __instance.transform.forward);
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

            static void ToolModeUpdate (ref FirstPersonManipulator ____firstPersonManipulator) {
                if (____firstPersonManipulator != _manipulator) {
                    ____firstPersonManipulator = _manipulator;
                }
            }

            static Vector3 _cameraForward;
            static Vector3 _cameraPosition;

            static void PreUpdateIsDroppable () {
                var camera = Locator.GetPlayerCamera();
                _cameraForward = camera.transform.forward;
                _cameraPosition = camera.transform.position;
                camera.transform.position = _laser.position;
                camera.transform.forward = _laser.forward;
            }

            static void PostUpdateIsDroppable () {
                var camera = Locator.GetPlayerCamera();
                camera.transform.position = _cameraPosition;
                camera.transform.forward = _cameraForward;
            }
        }

    }
}
