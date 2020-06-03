using System;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class LaserPointer : NomaiVRModule<LaserPointer.Behaviour, LaserPointer.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Transform Laser;
            private static FirstPersonManipulator _manipulator;
            private LineRenderer _lineRenderer;
            private bool isLongLine;

            private void Start()
            {
                Laser = new GameObject("Laser").transform;
                Laser.gameObject.AddComponent<FollowTarget>();
                Laser.transform.parent = HandsController.Behaviour.RightHand;
                Laser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
                Laser.transform.localRotation = Quaternion.Euler(45f, 0, 0);

                _lineRenderer = Laser.gameObject.AddComponent<LineRenderer>();
                _lineRenderer.useWorldSpace = false;
                _lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 0.5f });
                _lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
                _lineRenderer.startColor = Color.clear;
                _lineRenderer.startWidth = 0.005f;
                _lineRenderer.endWidth = 0.001f;
                _lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");

                FindObjectOfType<FirstPersonManipulator>().enabled = false;
                _manipulator = Laser.gameObject.AddComponent<FirstPersonManipulator>();

                DisableReticule();

                var selectables = Resources.FindObjectsOfTypeAll<Selectable>();
                foreach (var selectable in selectables)
                {
                    var collider = selectable.gameObject.AddComponent<BoxCollider>();
                    var rect = selectable.GetComponent<RectTransform>();
                    collider.size = new Vector3(rect.sizeDelta.x, rect.sizeDelta.y, rect.sizeDelta.x);
                }
            }

            private void Update()
            {
                if (_lineRenderer.enabled && ToolHelper.IsUsingAnyTool())
                {
                    _lineRenderer.enabled = false;
                }
                else if (!_lineRenderer.enabled && !ToolHelper.IsUsingAnyTool())
                {
                    _lineRenderer.enabled = true;
                }

                RaycastHit hit;
                if (Physics.Raycast(Laser.position, Laser.forward, out hit, 100))
                {
                    var selectable = hit.transform.GetComponent<Selectable>();
                    if (selectable != null)
                    {
                        selectable.Select();
                        if (OWInput.IsNewlyPressed(InputLibrary.interact))
                        {
                            var optionsSelector = selectable.GetComponent<OptionsSelectorElement>();
                            if (optionsSelector != null)
                            {
                                optionsSelector.OnArrowSelectableOnRightClick();
                                optionsSelector.OnArrowSelectableOnDownClick();
                            }
                        }
                    }
                }


            }

            private void DisableReticule()
            {
                var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var rootObject in rootObjects)
                {
                    if (rootObject.name == "Reticule")
                    {
                        rootObject.SetActive(false);
                        return;
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Pre<InteractZone>("UpdateInteractVolume", typeof(Patch), nameof(Patch.PatchUpdateInteractVolume));
                    NomaiVR.Pre<InteractZone>("OnEntry", typeof(Patch), nameof(Patch.InteractZoneEntry));
                    NomaiVR.Pre<InteractZone>("OnExit", typeof(Patch), nameof(Patch.InteractZoneExit));
                    NomaiVR.Pre<ToolModeSwapper>("Update", typeof(Patch), nameof(Patch.ToolModeUpdate));
                    NomaiVR.Pre<ItemTool>("UpdateIsDroppable", typeof(Patch), nameof(Patch.PreUpdateIsDroppable));
                    NomaiVR.Post<ItemTool>("UpdateIsDroppable", typeof(Patch), nameof(Patch.PostUpdateIsDroppable));
                }

                private static bool PatchUpdateInteractVolume(
                    InteractZone __instance,
                    float ____viewingWindow,
                    ref bool ____focused
                )
                {
                    var num = 2f * Vector3.Angle(Laser.forward, __instance.transform.forward);
                    var swapper = ToolHelper.Swapper;
                    var allowInteraction = swapper.IsInToolMode(ToolMode.None) || swapper.IsInToolMode(ToolMode.Item);
                    ____focused = allowInteraction && num <= ____viewingWindow;
                    var Base = __instance as SingleInteractionVolume;

                    var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
                    var ftn = method.MethodHandle.GetFunctionPointer();
                    var func = (Action)Activator.CreateInstance(typeof(Action), __instance, ftn);

                    func();

                    return false;
                }

                private static bool InteractZoneEntry(GameObject hitObj, InteractZone __instance)
                {
                    if (hitObj.CompareTag("PlayerDetector"))
                    {
                        _manipulator.OnEnterInteractZone(__instance);
                    }
                    return false;
                }

                private static bool InteractZoneExit(GameObject hitObj, InteractZone __instance)
                {
                    if (hitObj.CompareTag("PlayerDetector"))
                    {
                        _manipulator.OnExitInteractZone(__instance);
                    }
                    return false;
                }

                private static void ToolModeUpdate(ref FirstPersonManipulator ____firstPersonManipulator)
                {
                    if (____firstPersonManipulator != _manipulator)
                    {
                        ____firstPersonManipulator = _manipulator;
                    }
                }

                private static Quaternion _cameraRotation;
                private static Vector3 _cameraPosition;

                private static void PreUpdateIsDroppable()
                {
                    var camera = Locator.GetPlayerCamera();
                    _cameraRotation = camera.transform.rotation;
                    _cameraPosition = camera.transform.position;
                    camera.transform.position = Laser.position;
                    camera.transform.forward = Laser.forward;
                }

                private static void PostUpdateIsDroppable()
                {
                    var camera = Locator.GetPlayerCamera();
                    camera.transform.position = _cameraPosition;
                    camera.transform.rotation = _cameraRotation;
                }
            }

        }
    }
}