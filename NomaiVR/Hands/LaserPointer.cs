using NomaiVR.Input;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using static InputConsts;

namespace NomaiVR
{
    internal class LaserPointer : NomaiVRModule<LaserPointer.Behaviour, LaserPointer.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Behaviour Instance { get; private set; }
            public static Transform Laser;
            public static Transform OffHandLaser;
            public static Transform MovementLaser;
            public FirstPersonManipulator Manipulator => _manipulator;
            

            private static FirstPersonManipulator _manipulator;
            private LineRenderer _lineRenderer;
            private const float _gameLineLength = 0.5f;
            private const float _menuLineLength = 3f;
            private bool _rightMainHand;
            private bool _isReady;
            private OWMenuInputModule _inputModule;
            private DialogueBoxVer2 _dialogueBox;
            private PointerModelExposed _fakePointer;

            internal void Start()
            {
                Instance = this;
                SetUpLaserObject();
                SetUpOffHandLaser();
                ToDominantHand();
                SetUpLineRenderer();
                UpdateLineAppearance();
                CreateUIColliders();

                if (SceneHelper.IsInGame())
                {
                    SetUpFirstPersonManipulator();
                    _dialogueBox = FindObjectOfType<DialogueBoxVer2>();
                }

                if (SceneHelper.IsInTitle())
                {
                    SetUpTitleAnimationHandler();
                }

                ModSettings.OnConfigChange += ToDominantHand;
                VRToolSwapper.Equipped += OnToolEquipped;
                VRToolSwapper.UnEquipped += ToDominantHand;
                //FIXME: Change movement to movement hand
                //ControllerInput.Behaviour.BindingsChanged += UpdateMovementLaser;

                _fakePointer = new PointerModelExposed(new ExtendedPointerEventData(EventSystem.current));
                _inputModule = FindObjectOfType<OWMenuInputModule>();
            }

            internal void OnDestroy()
            {
                ModSettings.OnConfigChange -= ToDominantHand;
                VRToolSwapper.Equipped -= OnToolEquipped;
                VRToolSwapper.UnEquipped -= ToDominantHand;
                //ControllerInput.Behaviour.BindingsChanged -= UpdateMovementLaser;
            }

            internal void Update()
            {
                UpdateLineVisibility();
                UpdateLineAppearance();

                RaycastHit raycast = DoUIRaycast(_menuLineLength);

                if (raycast.transform != null)
                {
                    SetLineLength(raycast.distance);

                    //Dialogue handling
                    var dialogueOption = raycast.transform.GetComponent<DialogueOptionUI>();
                    if (dialogueOption != null)
                    {
                        HandleDialogueOptionHit(dialogueOption);
                    }

                    //Send fake events
                    _fakePointer.screenPosition = Camera.main.WorldToScreenPoint(raycast.point);
                    _fakePointer.leftButton.isPressed = SteamVR_Actions._default.UISelect.stateDown;
                    _fakePointer.changedThisFrame = SteamVR_Actions._default.UISelect.stateDown;
                    _inputModule.ProcessPointer(ref _fakePointer);
                }
            }

            private void HandleDialogueOptionHit(DialogueOptionUI dialogueOption)
            {
                if (_dialogueBox._revealingOptions)
                {
                    return;
                }
                var selectedOption = _dialogueBox.GetSelectedOption();
                var options = _dialogueBox._optionsUIElements;
                options[selectedOption].SetSelected(false);
                _dialogueBox._selectedOption = options.IndexOf(dialogueOption);
                dialogueOption.SetSelected(true);
            }

            private void SetUpLaserObject()
            {
                Laser = new GameObject("Laser").transform;
                Laser.gameObject.layer = LayerMask.NameToLayer("UI");
                Laser.localPosition = new Vector3(0f, -0.05f, 0.01f);
                Laser.localRotation = Quaternion.Euler(45f, 0, 0);
            }

            private static void SetUpOffHandLaser()
            {
                OffHandLaser = new GameObject("OffHandLaser").transform;
                OffHandLaser.localPosition = new Vector3(0f, -0.05f, 0.01f);
                OffHandLaser.localRotation = Quaternion.Euler(45f, 0, 0);
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

            private void SetUpTitleAnimationHandler()
            {
                var titleAnimationController = FindObjectOfType<TitleAnimationController>();
                titleAnimationController.OnTitleMenuAnimationComplete += () => _isReady = true;
            }

            private void ToDominantHand() => ForceHand(HandsController.Behaviour.DominantHand);
            private void ForceHand(Transform mainHand)
            {
                _rightMainHand = HandsController.Behaviour.RightHand == mainHand;
                Laser.transform.SetParent(mainHand, false);
                OffHandLaser.SetParent(_rightMainHand ? HandsController.Behaviour.LeftHand : HandsController.Behaviour.RightHand, false);
                UpdateMovementLaser();
            }

            private void UpdateMovementLaser()
            {
                var rightHandLaser = _rightMainHand ? Laser : OffHandLaser;
                var leftHandLaser = !_rightMainHand ? Laser : OffHandLaser;
                var movementOnLeftHand = InputMap.GetActionInput(InputCommandType.MOVE_X)?.Action?.activeDevice == SteamVR_Input_Sources.LeftHand;
                MovementLaser = movementOnLeftHand ? leftHandLaser : rightHandLaser;
            }

            private void OnToolEquipped()
            {
                if (VRToolSwapper.InteractingHand != null)
                    ForceHand(VRToolSwapper.InteractingHand.transform);
                else
                    ToDominantHand();
            }

            private void SetLineLength(float length)
            {
                _lineRenderer.SetPosition(1, Vector3.forward * length);

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
                    _lineRenderer.material.shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
                }
            }

            private void UpdateLineVisibility()
            {
                var isUsingTool = SceneHelper.IsInGame() && ToolHelper.IsUsingAnyTool(ToolGroup.Suit);
                if (_lineRenderer.enabled && isUsingTool)
                {
                    _lineRenderer.enabled = false;
                }
                else if (!_lineRenderer.enabled && InputHelper.IsUIInteractionMode(true))
                {
                    _lineRenderer.enabled = true;
                }
                else if (!isUsingTool && !InputHelper.IsUIInteractionMode(true) && _lineRenderer.enabled != ModSettings.EnableHandLaser)
                {
                    _lineRenderer.enabled = ModSettings.EnableHandLaser;
                }
            }

            public RaycastHit DoUIRaycast(float distance)
            {
                if (!_isReady || !InputHelper.IsUIInteractionMode(true) || LoadManager.IsBusy())
                {
                    return default;
                }

                Physics.Raycast(Laser.position, Laser.forward, out var hit, distance, LayerMask.GetMask("UI"));

                return hit;
            }

            /// <summary>
            /// Creates box colliders for all canvases that have at least one selectable item
            /// </summary>
            private void CreateUIColliders()
            {
                var selectables = Resources.FindObjectsOfTypeAll<Selectable>();
                foreach (var selectable in selectables)
                {
                    if (selectable.targetGraphic != null &&
                       selectable.targetGraphic.canvas != null)
                    {
                        SetupInteractableCanvasCollider(selectable.targetGraphic.canvas);
                    }
                }
            }

            private void SetupInteractableCanvasCollider(Canvas canvas, GameObject proxy = null)
            {
                if (proxy == null) proxy = canvas.gameObject;
                var collider = proxy.GetComponent<BoxCollider>();
                if(collider == null)
                {
                    var rectTransform = canvas.GetComponent<RectTransform>();
                    var thickness = 0.1f;
                    collider = proxy.gameObject.AddComponent<BoxCollider>();
                    collider.size = rectTransform.sizeDelta;
                    collider.center = new Vector3(0, 0, thickness * 0.5f);
                    proxy.layer = LayerMask.NameToLayer("UI");
                    canvas.worldCamera = Camera.main;
                }
            }

            public class Patch : NomaiVRPatch
            {
                private static IntPtr pointerUpdateInteractVolume;

                public override void ApplyPatches()
                {
                    Prefix<InteractZone>(nameof(InteractZone.OnEntry), nameof(PreInteractZoneEntry));
                    Prefix<InteractZone>(nameof(InteractZone.OnExit), nameof(PreInteractZoneExit));
                    Prefix<ToolModeSwapper>(nameof(ToolModeSwapper.Update), nameof(PreToolModeUpdate));
                    Prefix<ItemTool>(nameof(ItemTool.UpdateIsDroppable), nameof(PreUpdateIsDroppable));
                    Postfix<ItemTool>(nameof(ItemTool.UpdateIsDroppable), nameof(PostUpdateIsDroppable));

                    pointerUpdateInteractVolume = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume").MethodHandle.GetFunctionPointer();
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
