using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NomaiVR
{
    public class LaserPointer : NomaiVRModule<LaserPointer.Behaviour, LaserPointer.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => AllScenes;

        public static bool IsUIPointer()
        {
            return OWInput.IsInputMode(InputMode.Menu | InputMode.KeyboardInput);
        }

        public class Behaviour : MonoBehaviour
        {
            public static Transform Laser;
            private static FirstPersonManipulator _manipulator;
            private LineRenderer _lineRenderer;
            private const float _gameLineLength = 0.5f;
            private const float _menuLineLength = 2f;
            private TabButton[] _tabButtons;
            private bool _isReady;

            private void Start()
            {
                Laser = new GameObject("Laser").transform;
                Laser.gameObject.layer = LayerMask.NameToLayer("UI");
                Laser.gameObject.AddComponent<FollowTarget>();
                Laser.transform.parent = HandsController.Behaviour.RightHand;
                Laser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
                Laser.transform.localRotation = Quaternion.Euler(45f, 0, 0);

                _lineRenderer = Laser.gameObject.AddComponent<LineRenderer>();
                _lineRenderer.useWorldSpace = false;
                _lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
                _lineRenderer.startWidth = 0.005f;
                _lineRenderer.endWidth = 0.001f;
                _lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
                _lineRenderer.startColor = Color.clear;
                UpdateLineAppearance();

                DisableReticule();
                CreateButtonColliders();

                if (SceneHelper.IsInGame())
                {
                    FindObjectOfType<FirstPersonManipulator>().enabled = false;
                    _manipulator = Laser.gameObject.AddComponent<FirstPersonManipulator>();
                    _isReady = true;
                }

                if (SceneHelper.IsInTitle())
                {
                    var titleAnimationController = FindObjectOfType<TitleAnimationController>();
                    titleAnimationController.OnTitleMenuAnimationComplete += OnTitleMenuAnimationComplete;
                }
            }

            private void CreateButtonColliders()
            {
                _tabButtons = Resources.FindObjectsOfTypeAll<TabButton>();

                var selectables = Resources.FindObjectsOfTypeAll<Selectable>();
                foreach (var selectable in selectables)
                {
                    var tooltipSelectable = selectable.GetComponent<TooltipSelectable>();
                    if (tooltipSelectable != null)
                    {
                        // Move children to avoid ray z-fighting;
                        foreach (Transform child in selectable.transform)
                        {
                            child.localPosition += Vector3.forward;
                        }
                    }

                    var collider = selectable.gameObject.AddComponent<BoxCollider>();

                    var rectTransform = selectable.GetComponent<RectTransform>();
                    var thickness = 10f;
                    var height = Math.Max(60f, rectTransform.rect.height);
                    var width = Math.Max(60f, rectTransform.rect.width);
                    collider.size = new Vector3(width, height, thickness);
                    collider.center = new Vector3(0, 0, thickness * 0.5f);
                }
            }

            private void OnTitleMenuAnimationComplete()
            {
                _isReady = true;
            }

            private void UpdateUiRayCast()
            {
                if (!_isReady || !LaserPointer.IsUIPointer())
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
                if (LaserPointer.IsUIPointer())
                {
                    SetLineLength(_menuLineLength);
                    _lineRenderer.material.shader = Shader.Find("Unlit/Color");
                    _lineRenderer.material.SetColor("_Color", new Color(0.8f, 0.8f, 0.8f));
                }
                else
                {
                    SetLineLength(_gameLineLength);
                    _lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");
                }
            }

            private void UpdateLineVisibility()
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
            }

            private void Update()
            {
                UpdateLineVisibility();
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