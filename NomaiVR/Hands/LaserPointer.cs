using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class LaserPointer : NomaiVRModule<LaserPointer.Behaviour, LaserPointer.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Transform Laser;
            public static Transform LeftHandLaser;
            private static FirstPersonManipulator _manipulator;
            private LineRenderer _lineRenderer;
            private const float _gameLineLength = 0.5f;
            private const float _menuLineLength = 2f;
            private TabButton[] _tabButtons;
            private bool _isReady;
            private Transform _prevRayHit;
            private DialogueBoxVer2 _dialogueBox;
            private PauseMenuManager _pauseMenuManager;

            internal void Start()
            {
                SetUpLaserObject();
                SetUpLeftHandLaser();
                SetUpLineRenderer();
                UpdateLineAppearance();
                DisableReticule();
                CreateButtonColliders();

                if (SceneHelper.IsInGame())
                {
                    SetUpFirstPersonManipulator();
                    SetUpDialogueOptions();
                    _pauseMenuManager = FindObjectOfType<PauseMenuManager>();
                }

                if (SceneHelper.IsInTitle())
                {
                    SetUpTitleAnimationHandler();
                }
            }

            internal void Update()
            {
                UpdateLineVisibility();
                UpdateLineAppearance();
                UpdateUiRayCast();
                UpdateAlternativeButtons();

                if (IsSelectNewlyPressed())
                {
                    HandleTransformClick(_prevRayHit);
                }
            }

            private static void SetUpLaserObject()
            {
                Laser = new GameObject("Laser").transform;
                Laser.gameObject.layer = LayerMask.NameToLayer("UI");
                Laser.transform.parent = HandsController.Behaviour.RightHand;
                Laser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
                Laser.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            }

            private static void SetUpLeftHandLaser()
            {
                LeftHandLaser = new GameObject("LeftHandLaser").transform;
                LeftHandLaser.transform.parent = HandsController.Behaviour.LeftHand;
                LeftHandLaser.transform.localPosition = new Vector3(0f, -0.05f, 0.01f);
                LeftHandLaser.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            }

            private void SetUpLineRenderer()
            {
                _lineRenderer = Laser.gameObject.AddComponent<LineRenderer>();
                _lineRenderer.useWorldSpace = false;
                _lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
                _lineRenderer.startWidth = 0.005f;
                _lineRenderer.endWidth = 0.001f;
                _lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
                _lineRenderer.startColor = Color.clear;
            }

            private void SetUpFirstPersonManipulator()
            {
                FindObjectOfType<FirstPersonManipulator>().enabled = false;
                _manipulator = Laser.gameObject.AddComponent<FirstPersonManipulator>();
                _isReady = true;
            }

            private void SetUpDialogueOptions()
            {
                _dialogueBox = FindObjectOfType<DialogueBoxVer2>();
            }

            private void SetUpTitleAnimationHandler()
            {
                var titleAnimationController = FindObjectOfType<TitleAnimationController>();
                titleAnimationController.OnTitleMenuAnimationComplete += () => _isReady = true;
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

            private static bool IsSelectNewlyPressed()
            {
                return OWInput.IsNewlyPressed(InputLibrary.menuConfirm);
            }

            private void HandleSelectableRayHit(Selectable selectable)
            {
                var tab = selectable.transform.GetComponent<TabButton>();
                if (tab == null)
                {
                    selectable.Select();
                }
                else
                {
                    DeselectAllTabs();
                    tab.OnPointerEnter(null);
                }
            }

            private static void HandleOptionsSelectorClick(OptionsSelectorElement optionsSelector)
            {
                optionsSelector.OnArrowSelectableOnRightClick();
                optionsSelector.OnArrowSelectableOnDownClick();
            }

            private static void HandleTwoButtonToggleClick(TwoButtonToggleElement twoButtonToggle)
            {
                var selection = twoButtonToggle.GetValue();
                twoButtonToggle.SetValue("_selection", !selection);
                twoButtonToggle.Invoke("UpdateToggleColors");
            }

            private static void HandleSliderClick(Slider slider)
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

            private static void HandleButtonBlick(Button button)
            {
                button.onClick.Invoke();
            }

            private static void HandleSelectableClick(Selectable selectable)
            {
                var optionsSelector = selectable.GetComponent<OptionsSelectorElement>();
                if (optionsSelector != null)
                {
                    HandleOptionsSelectorClick(optionsSelector);
                    return;
                }

                var twoButtonToggle = selectable.GetComponent<TwoButtonToggleElement>();
                if (twoButtonToggle != null)
                {
                    HandleTwoButtonToggleClick(twoButtonToggle);
                    return;
                }

                var slider = selectable.GetComponentInChildren<Slider>();
                if (slider != null)
                {
                    HandleSliderClick(slider);
                    return;
                }

                var button = selectable.GetComponent<Button>();
                if (button != null)
                {
                    HandleButtonBlick(button);
                    return;
                }
            }

            private static void HandleTabClick(TabButton tab)
            {
                tab.OnSelect(null);
            }

            private void HandleTransformClick(Transform clickTransform)
            {
                if (clickTransform == null)
                {
                    return;
                }
                var selectable = clickTransform.GetComponent<Selectable>();
                if (selectable == null)
                {
                    return;
                }
                var tab = selectable.transform.GetComponent<TabButton>();
                if (tab == null)
                {
                    HandleSelectableClick(selectable);
                }
                else
                {
                    HandleTabClick(tab);
                }
                return;
            }

            private void HandleDialogueOptionHit(DialogueOptionUI dialogueOption)
            {
                if (_dialogueBox.GetValue<bool>("_revealingOptions"))
                {
                    return;
                }
                var selectedOption = _dialogueBox.GetSelectedOption();
                var options = _dialogueBox.GetValue<List<DialogueOptionUI>>("_optionsUIElements");
                options[selectedOption].SetSelected(false);
                _dialogueBox.SetValue("_selectedOption", options.IndexOf(dialogueOption));
                dialogueOption.SetSelected(true);
            }

            private void DeselectAllTabs()
            {
                foreach (var tabButton in _tabButtons)
                {
                    tabButton.OnPointerExit(null);
                }
            }

            private bool HandleTransformHit(Transform hitTransform)
            {
                if (hitTransform == null)
                {
                    return false;
                }
                var selectable = hitTransform.GetComponent<Selectable>();
                if (selectable != null)
                {
                    HandleSelectableRayHit(selectable);
                    return true;
                }
                var dialogueOption = hitTransform.GetComponent<DialogueOptionUI>();
                if (dialogueOption != null)
                {
                    HandleDialogueOptionHit(dialogueOption);
                    return true;
                }
                return false;
            }

            private void UpdateUiRayCast()
            {
                if (!_isReady || !InputHelper.IsUIInteractionMode(true) || LoadManager.IsBusy())
                {
                    return;
                }

                if (Physics.Raycast(Laser.position, Laser.forward, out var hit, _menuLineLength, LayerMask.GetMask("UI")))
                {
                    SetLineLength(hit.distance);
                    if (hit.transform == _prevRayHit && !IsSelectNewlyPressed())
                    {
                        return;
                    }
                    _prevRayHit = hit.transform;

                    if (HandleTransformHit(hit.transform))
                    {
                        return;
                    }
                }
                else
                {
                    DeselectAllTabs();
                }
            }

            private void SetLineLength(float length)
            {
                _lineRenderer.SetPosition(1, Vector3.forward * length);

            }

            private static void DisableReticule()
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

            private void UpdateLineAppearance()
            {
                if (InputHelper.IsUIInteractionMode(true))
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
                var isUsingTool = SceneHelper.IsInGame() && ToolHelper.IsUsingAnyTool(ToolGroup.Suit);
                if (_lineRenderer.enabled && isUsingTool)
                {
                    _lineRenderer.enabled = false;
                }
                else if (!_lineRenderer.enabled && !isUsingTool)
                {
                    _lineRenderer.enabled = true;
                }
            }

            private bool IsPaused()
            {
                return _pauseMenuManager != null && _pauseMenuManager.IsOpen();
            }

            private void UpdateAlternativeButtons()
            {
                var hasPressedConfirm = OWInput.IsNewlyPressed(InputLibrary.menuConfirm);
                var hasPressedThrustUp = OWInput.IsNewlyPressed(InputLibrary.thrustUp);
                var hasPressedInteract = OWInput.IsNewlyPressed(InputLibrary.interact);
                var isTitleScreen = LoadManager.GetCurrentScene() == OWScene.TitleScreen;

                if (PlayerState.InConversation() && (hasPressedConfirm || hasPressedThrustUp))
                {
                    ControllerInput.Behaviour.SimulateInput(JoystickButton.FaceLeft);
                }
                if ((IsPaused() || isTitleScreen) && (hasPressedInteract || hasPressedThrustUp))
                {
                    ControllerInput.Behaviour.SimulateInput(JoystickButton.FaceDown);
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<InteractZone>("UpdateInteractVolume", nameof(PreUpdateInteractVolume));
                    Prefix<InteractZone>("OnEntry", nameof(PreInteractZoneEntry));
                    Prefix<InteractZone>("OnExit", nameof(PreInteractZoneExit));
                    Prefix<ToolModeSwapper>("Update", nameof(PreToolModeUpdate));
                    Prefix<ItemTool>("UpdateIsDroppable", nameof(PreUpdateIsDroppable));
                    Postfix<ItemTool>("UpdateIsDroppable", nameof(PostUpdateIsDroppable));
                }

                private static bool PreUpdateInteractVolume(
                    InteractZone __instance,
                    float ____viewingWindow,
                    ref bool ____focused
                )
                {
                    var num = 2f * Vector3.Angle(Laser.forward, __instance.transform.forward);
                    var allowInteraction = ToolHelper.IsUsingNoTools();
                    ____focused = allowInteraction && num <= ____viewingWindow;
                    var Base = __instance as SingleInteractionVolume;

                    var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
                    var ftn = method.MethodHandle.GetFunctionPointer();
                    var func = (Action)Activator.CreateInstance(typeof(Action), __instance, ftn);

                    func();

                    return false;
                }

                private static bool PreInteractZoneEntry(GameObject hitObj, InteractZone __instance)
                {
                    if (hitObj.CompareTag("PlayerDetector"))
                    {
                        _manipulator.OnEnterInteractZone(__instance);
                    }
                    return false;
                }

                private static bool PreInteractZoneExit(GameObject hitObj, InteractZone __instance)
                {
                    if (hitObj.CompareTag("PlayerDetector"))
                    {
                        _manipulator.OnExitInteractZone(__instance);
                    }
                    return false;
                }

                private static void PreToolModeUpdate(ref FirstPersonManipulator ____firstPersonManipulator)
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