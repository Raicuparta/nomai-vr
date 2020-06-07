using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class VRTutorial : NomaiVRModule<VRTutorial.Behaviour, VRTutorial.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Dictionary<InputCommand, TutorialInput> _tutorialInputs;
            private static List<TutorialInput> _queue;
            private List<SteamVR_RenderModel> _controllerModels;
            private bool _isShowingControlleModels;

            private void Start()
            {
                _queue = new List<TutorialInput>();
                _controllerModels = new List<SteamVR_RenderModel>();

                CreateControllerModel(HandsController.Behaviour.RightHand);
                CreateControllerModel(HandsController.Behaviour.LeftHand);
                Invoke(nameof(HideControllerModels), 0.1f);

                var actions = SteamVR_Actions._default;
                _tutorialInputs = new Dictionary<InputCommand, TutorialInput>();

                var interact = new TutorialInput("interact", actions.Interact, 0);
                _tutorialInputs[InputLibrary.interact] = interact;
                _tutorialInputs[InputLibrary.translate] = interact;
                _tutorialInputs[InputLibrary.scopeView] = interact;
                _tutorialInputs[InputLibrary.probeForward] = interact;
                _tutorialInputs[InputLibrary.lockOn] = interact;

                var holdInteract = new TutorialInput("holdInteract", actions.Interact, 1);
                _tutorialInputs[InputLibrary.suitMenu] = holdInteract;
                _tutorialInputs[InputLibrary.probeRetrieve] = holdInteract;
                _tutorialInputs[InputLibrary.sleep] = holdInteract;
                _tutorialInputs[InputLibrary.swapShipLogMode] = holdInteract;
                _tutorialInputs[InputLibrary.autopilot] = holdInteract;

                var jump = new TutorialInput("jump", actions.Jump, 2);
                _tutorialInputs[InputLibrary.jump] = jump;
                _tutorialInputs[InputLibrary.markEntryOnHUD] = jump;

                _tutorialInputs[InputLibrary.matchVelocity] = new TutorialInput("matchVelocity", actions.Jump, 3);
                _tutorialInputs[InputLibrary.boost] = new TutorialInput("boost", actions.Jump, 3);

                _tutorialInputs[InputLibrary.map] = new TutorialInput("map", actions.Map, 3);

                var zeroGLook = new TutorialInput("zeroGLook", actions.Look, 7);
                _tutorialInputs[InputLibrary.yaw] = zeroGLook;
                _tutorialInputs[InputLibrary.pitch] = zeroGLook;

                _tutorialInputs[InputLibrary.extendStick] = new TutorialInput("extendStick", actions.ThrustUp, 0);
                _tutorialInputs[InputLibrary.thrustUp] = new TutorialInput("thrustUp", actions.ThrustUp, 4);
                _tutorialInputs[InputLibrary.thrustDown] = new TutorialInput("thrustDown", actions.ThrustDown, 5);

                _tutorialInputs[InputLibrary.rollMode] = new TutorialInput("rollMode", actions.RollMode, 8);

                _tutorialInputs[InputLibrary.probeReverse] = new TutorialInput("probeReverse", actions.RollMode, 8);

                _tutorialInputs[InputLibrary.cancel] = new TutorialInput("back", actions.Back, 9);

                // Show these right away instead of waiting for a prompt.
                var move = new TutorialInput("move", actions.Move, 6);
                var look = new TutorialInput("look", actions.Look, 7);
                AddToQueue(move);
                AddToQueue(look);
            }

            private static void AddToQueue(TutorialInput input)
            {
                _queue.Add(input);
                _queue.Sort((a, b) => a.priority - b.priority);
            }

            private void Update()
            {
                if (_queue.Count > 0)
                {
                    _queue[0].Show();
                }
                if (!_isShowingControlleModels && _queue.Count > 0)
                {
                    ShowControllerModels();
                }
                else if (_isShowingControlleModels && _queue.Count == 0)
                {
                    HideControllerModels();
                }
            }

            private void ShowControllerModels()
            {
                _isShowingControlleModels = true;
                _controllerModels.ForEach(model =>
                {
                    model.enabled = true;
                    model.gameObject.SetActive(true);
                });
            }

            private void HideControllerModels()
            {
                _isShowingControlleModels = false;
                _controllerModels.ForEach(model =>
                {
                    model.enabled = false;
                    model.gameObject.SetActive(false);
                });
            }

            private void CreateControllerModel(Transform hand)
            {
                var controllerModel = new GameObject().AddComponent<SteamVR_RenderModel>();
                controllerModel.updateDynamically = false;
                controllerModel.createComponents = false;
                controllerModel.transform.parent = hand;
                controllerModel.transform.localPosition = Vector3.zero;
                controllerModel.transform.localRotation = Quaternion.identity;

                _controllerModels.Add(controllerModel);
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ScreenPrompt>("SetVisibility", typeof(Patch), nameof(SetPromptVisibility));
                }

                private static void SetPromptVisibility(bool isVisible, List<InputCommand> ____commandList)
                {
                    if (_tutorialInputs == null || _queue == null)
                    {
                        return;
                    }
                    foreach (var command in ____commandList)
                    {
                        if (isVisible && _tutorialInputs.ContainsKey(command))
                        {
                            var tutorialInput = _tutorialInputs[command];
                            if (tutorialInput == null)
                            {
                                continue;
                            }

                            if (!tutorialInput.isDone && !_queue.Contains(tutorialInput))
                            {
                                AddToQueue(_tutorialInputs[command]);
                                _queue.Sort((a, b) => a.priority - b.priority);
                            }
                        }
                    }
                }
            }

            private class TutorialInput
            {
                public bool isDone;
                public SteamVR_Action action;
                public bool isShowing;
                public int priority;
                private readonly string name;

                public TutorialInput(string name, SteamVR_Action action, int priority)
                {
                    this.name = name;
                    this.action = action;
                    this.priority = priority;
                    action.HideOrigins();

                    if (NomaiVR.Save.tutorialSteps.Contains(name))
                    {
                        isDone = true;
                    }

                    if (action is SteamVR_Action_Vector2)
                    {
                        ((SteamVR_Action_Vector2)action).onChange += OnChange;
                    }
                    if (action is SteamVR_Action_Single)
                    {
                        ((SteamVR_Action_Single)action).onChange += OnChange;
                    }
                    if (action is SteamVR_Action_Boolean)
                    {
                        ((SteamVR_Action_Boolean)action).onChange += OnChange;
                    }
                }

                private void OnChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
                {
                    OnChange();
                }

                private void OnChange(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta)
                {
                    OnChange();
                }

                private void OnChange(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
                {
                    OnChange();
                }

                private void OnChange()
                {
                    if (isShowing)
                    {
                        Hide();
                    }
                }

                public void Hide()
                {
                    action.HideOrigins();

                    TimerHelper.ExecuteAfter(() =>
                    {
                        isShowing = false;
                        isDone = true;
                        _queue.Remove(this);
                    }, 500);
                    NomaiVR.Save.AddTutorialStep(name);
                }

                public void Show()
                {
                    if (isDone)
                    {
                        _queue.Remove(this);
                        return;
                    }
                    if (isShowing)
                    {
                        return;
                    }
                    isShowing = true;
                    action.ShowOrigins();
                }
            }
        }
    }
}
