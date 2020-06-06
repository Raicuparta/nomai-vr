using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class LaserPointer : NomaiVRModule<LaserPointer.Behaviour, LaserPointer.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Transform Laser;
            private static FirstPersonManipulator _manipulator;
            private LineRenderer _lineRenderer;
            private const float _gameLineLength = 0.5f;
            private const float _menuLineLength = 1.5f;
            private TabButton[] _tabButtons;

            private void Start()
            {
                Laser = new GameObject("Laser").transform;
                Laser.gameObject.AddComponent<FollowTarget>();
                Laser.transform.parent = HandsController.Behaviour.RightHand;
                Laser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
                Laser.transform.localRotation = Quaternion.Euler(45f, 0, 0);

                _lineRenderer = Laser.gameObject.AddComponent<LineRenderer>();
                _lineRenderer.useWorldSpace = false;
                _lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
                _lineRenderer.startWidth = 0.005f;
                _lineRenderer.endWidth = 0.001f;
                _lineRenderer.material = Canvas.GetDefaultCanvasMaterial();
                UpdateLineAppearance();

                DisableReticule();

                var selectables = Resources.FindObjectsOfTypeAll<Selectable>();
                foreach (var selectable in selectables)
                {
                    var collider = selectable.gameObject.AddComponent<BoxCollider>();
                    var rectTransform = selectable.GetComponent<RectTransform>();
                    var thickness = 100f;
                    var height = Math.Max(60f, rectTransform.rect.height);
                    var width = Math.Max(60f, rectTransform.rect.width);
                    collider.size = new Vector3(width, height, rectTransform.rect.height);
                    collider.center = new Vector3(0, 0, thickness * 0.5f);
                }

                _tabButtons = Resources.FindObjectsOfTypeAll<TabButton>();

                if (SceneHelper.IsInGame())
                {
                    FindObjectOfType<FirstPersonManipulator>().enabled = false;
                    _manipulator = Laser.gameObject.AddComponent<FirstPersonManipulator>();
                }
            }

            private void UpdateUiRayCast()
            {
                if (OWInput.GetInputMode() != InputMode.Menu)
                {
                    return;
                }

                foreach (var tabButton in _tabButtons)
                {
                    tabButton.OnPointerExit(null);
                }

                if (Physics.Raycast(Laser.position, Laser.forward, out var hit, _menuLineLength, LayerMask.GetMask("UI")))
                {
                    var selectable = hit.transform.GetComponent<Selectable>();
                    if (selectable != null)
                    {
                        SetLineLength(hit.distance);
                        var tab = hit.transform.GetComponent<TabButton>();
                        if (tab != null)
                        {
                            tab.OnPointerEnter(null);
                        }
                        else
                        {
                            selectable.Select();
                        }
                        if (OWInput.IsNewlyPressed(InputLibrary.interact))
                        {
                            var optionsSelector = selectable.GetComponent<OptionsSelectorElement>();
                            if (optionsSelector != null)
                            {
                                optionsSelector.OnArrowSelectableOnRightClick();
                                optionsSelector.OnArrowSelectableOnDownClick();
                            }

                            var twoButtonToggle = selectable.GetComponent<TwoButtonToggleElement>();
                            if (twoButtonToggle != null)
                            {
                                var selection = twoButtonToggle.GetValue();
                                twoButtonToggle.SetValue("_selection", !selection);
                                twoButtonToggle.Invoke("UpdateToggleColors");
                            }

                            var slider = selectable.GetComponentInChildren<Slider>();
                            if (slider != null)
                            {
                                if (slider.value < slider.maxValue)
                                {
                                    slider.value += 1;
                                }
                                else
                                {
                                    slider.value = slider.minValue;
                                }
                            }
                            if (tab != null)
                            {
                                selectable.Select();
                            }
                        }
                    }
                }
            }

            private void SetLineLength(float length)
            {
                _lineRenderer.SetPosition(1, Vector3.forward * length);

            }

            private void UpdateLineAppearance()
            {
                if (OWInput.IsInputMode(InputMode.Menu))
                {
                    SetLineLength(_menuLineLength);
                    _lineRenderer.endColor = Color.white;
                    _lineRenderer.startColor = new Color(1, 1, 1, 0.5f);
                }
                else
                {
                    SetLineLength(_gameLineLength);
                    _lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
                    _lineRenderer.startColor = Color.clear;
                }
            }

            private void Update()
            {
                var isUsingTool = SceneHelper.IsInGame() && ToolHelper.IsUsingAnyTool();
                if (_lineRenderer.enabled && isUsingTool)
                {
                    _lineRenderer.enabled = false;
                }
                else if (!_lineRenderer.enabled && !isUsingTool)
                {
                    _lineRenderer.enabled = true;
                }

                UpdateLineAppearance();
                UpdateUiRayCast();
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