using System;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.ModConfig;
using NomaiVR.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.Hands
{
    internal class LaserPointer : NomaiVRModule<LaserPointer.Behaviour, LaserPointer.Behaviour.Patch>
    {
        private static readonly int color = Shader.PropertyToID("_Color");
        private static readonly Shader brightShader = Shader.Find("Unlit/Color");
        private static readonly Shader fadedShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");
        private static readonly Color brightColor = new Color(0.8f, 0.8f, 0.8f);
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Transform Laser;
            public static Transform OffHandLaser;
            public static Transform MovementLaser;
            public FirstPersonManipulator Manipulator => manipulator;
            

            private static FirstPersonManipulator manipulator;
            private LineRenderer lineRenderer;
            private const float gameLineLength = 0.5f;
            private const float menuLineLength = 3f;
            private bool rightMainHand;
            private OWMenuInputModule inputModule;
            private DialogueBoxVer2 dialogueBox;
            private PointerModelExposed fakePointer;

            internal void Start()
            {
                SetUpLaserObject();
                SetUpOffHandLaser();
                ToDominantHand();
                SetUpLineRenderer();
                UpdateLineAppearance();

                if (!SceneHelper.IsInCreditsScene())
                {
                    CreateUIColliders();
                }

                if (SceneHelper.IsInGame())
                {
                    SetUpFirstPersonManipulator();
                    dialogueBox = FindObjectOfType<DialogueBoxVer2>();
                }

                ModSettings.OnConfigChange += ToDominantHand;
                VRToolSwapper.Equipped += OnToolEquipped;
                VRToolSwapper.UnEquipped += ToDominantHand;
                //FIXME: Change movement to movement hand
                //ControllerInput.Behaviour.BindingsChanged += UpdateMovementLaser;

                fakePointer = new PointerModelExposed(new ExtendedPointerEventData(EventSystem.current));
                inputModule = FindObjectOfType<OWMenuInputModule>();
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

                var raycast = DoUIRaycast(menuLineLength);

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
                    fakePointer.screenPosition = Camera.main.WorldToScreenPoint(raycast.point);
                    fakePointer.leftButton.isPressed = SteamVR_Actions._default.UISelect.stateDown;
                    fakePointer.changedThisFrame = SteamVR_Actions._default.UISelect.stateDown;
                    inputModule.ProcessPointer(ref fakePointer);
                }
            }

            private void HandleDialogueOptionHit(DialogueOptionUI dialogueOption)
            {
                if (dialogueBox._revealingOptions)
                {
                    return;
                }
                var selectedOption = dialogueBox.GetSelectedOption();
                var options = dialogueBox._optionsUIElements;
                options[selectedOption].SetSelected(false);
                dialogueBox._selectedOption = options.IndexOf(dialogueOption);
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
                lineRenderer = Laser.gameObject.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
                lineRenderer.startWidth = 0.005f;
                lineRenderer.endWidth = 0.001f;
                lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
                lineRenderer.startColor = Color.clear;
            }

            private void SetUpFirstPersonManipulator()
            {
                FindObjectOfType<FirstPersonManipulator>().enabled = false;
                manipulator = Laser.gameObject.AddComponent<FirstPersonManipulator>();
            }

            private void ToDominantHand() => ForceHand(HandsController.Behaviour.DominantHand);
            private void ForceHand(Transform mainHand)
            {
                rightMainHand = HandsController.Behaviour.RightHand == mainHand;
                Laser.transform.SetParent(mainHand, false);
                OffHandLaser.SetParent(rightMainHand ? HandsController.Behaviour.LeftHand : HandsController.Behaviour.RightHand, false);
                UpdateMovementLaser();
            }

            private void UpdateMovementLaser()
            {
                var rightHandLaser = rightMainHand ? Laser : OffHandLaser;
                var leftHandLaser = !rightMainHand ? Laser : OffHandLaser;
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
                lineRenderer.SetPosition(1, Vector3.forward * length);

            }

            private void UpdateLineAppearance()
            {
                SetLineLength(InputHelper.IsUIInteractionMode(true) ? menuLineLength : gameLineLength);
                
                if (InputHelper.IsUIInteractionMode(true) || (manipulator && manipulator.HasFocusedInteractible()))
                {
                    lineRenderer.material.shader = brightShader;
                    lineRenderer.material.SetColor(color, brightColor);
                }
                else
                {
                    lineRenderer.material.shader = fadedShader;
                }
            }

            private void UpdateLineVisibility()
            {
                var isUsingTool = SceneHelper.IsInGame() && ToolHelper.IsUsingAnyTool(ToolGroup.Suit);
                if (lineRenderer.enabled && isUsingTool)
                {
                    lineRenderer.enabled = false;
                }
                else if (!lineRenderer.enabled && InputHelper.IsUIInteractionMode(true))
                {
                    lineRenderer.enabled = true;
                }
                else if (!isUsingTool && !InputHelper.IsUIInteractionMode(true) && lineRenderer.enabled != ModSettings.EnableHandLaser)
                {
                    lineRenderer.enabled = ModSettings.EnableHandLaser;
                }
            }

            public RaycastHit DoUIRaycast(float distance)
            {
                if (!InputHelper.IsUIInteractionMode(true) || LoadManager.IsBusy())
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
                        manipulator.OnEnterInteractZone(__instance);
                    }
                    return false;
                }

                private static bool PreInteractZoneExit(GameObject hitObj, InteractZone __instance)
                {
                    if (hitObj.CompareTag("PlayerDetector"))
                    {
                        manipulator.OnExitInteractZone(__instance);
                    }
                    return false;
                }

                private static void PreToolModeUpdate(ref FirstPersonManipulator ____firstPersonManipulator)
                {
                    if (____firstPersonManipulator != manipulator)
                    {
                        ____firstPersonManipulator = manipulator;
                    }
                }

                private static Quaternion cameraRotation;
                private static Vector3 cameraPosition;

                private static void PreUpdateIsDroppable()
                {
                    var camera = Locator.GetPlayerCamera();
                    cameraRotation = camera.transform.rotation;
                    cameraPosition = camera.transform.position;
                    camera.transform.position = Laser.position;
                    camera.transform.forward = Laser.forward;
                }

                private static void PostUpdateIsDroppable()
                {
                    var camera = Locator.GetPlayerCamera();
                    camera.transform.position = cameraPosition;
                    camera.transform.rotation = cameraRotation;
                }
            }
        }
    }
}
