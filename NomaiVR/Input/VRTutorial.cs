using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    class VRTutorial : MonoBehaviour
    {
        static Dictionary<InputCommand, TutorialInput> tutorialInputs;
        static List<TutorialInput> queue;
        List<SteamVR_RenderModel> controllerModels;
        bool isShowingControlleModels;

        void Start()
        {
            NomaiVR.Log("Start VRTutorial");

            queue = new List<TutorialInput>();
            controllerModels = new List<SteamVR_RenderModel>();

            CreateControllerModel(HandsController.RightHand);
            CreateControllerModel(HandsController.LeftHand);
            Invoke(nameof(HideControllerModels), 0.1f);

            var actions = SteamVR_Actions._default;
            tutorialInputs = new Dictionary<InputCommand, TutorialInput>();

            var interact = new TutorialInput("interact", actions.Interact, 0);
            tutorialInputs[InputLibrary.interact] = interact;
            tutorialInputs[InputLibrary.translate] = interact;
            tutorialInputs[InputLibrary.scopeView] = interact;
            tutorialInputs[InputLibrary.probeForward] = interact;
            tutorialInputs[InputLibrary.lockOn] = interact;

            var holdInteract = new TutorialInput("holdInteract", actions.Interact, 1);
            tutorialInputs[InputLibrary.suitMenu] = holdInteract;
            tutorialInputs[InputLibrary.probeRetrieve] = holdInteract;
            tutorialInputs[InputLibrary.sleep] = holdInteract;
            tutorialInputs[InputLibrary.swapShipLogMode] = holdInteract;
            tutorialInputs[InputLibrary.autopilot] = holdInteract;

            var jump = new TutorialInput("jump", actions.Jump, 2);
            tutorialInputs[InputLibrary.jump] = jump;
            tutorialInputs[InputLibrary.markEntryOnHUD] = jump;

            tutorialInputs[InputLibrary.matchVelocity] = new TutorialInput("matchVelocity", actions.Jump, 3);
            tutorialInputs[InputLibrary.boost] = new TutorialInput("boost", actions.Jump, 3);

            tutorialInputs[InputLibrary.map] = new TutorialInput("map", actions.Map, 3);

            var zeroGLook = new TutorialInput("zeroGLook", actions.Look, 7);
            tutorialInputs[InputLibrary.yaw] = zeroGLook;
            tutorialInputs[InputLibrary.pitch] = zeroGLook;

            tutorialInputs[InputLibrary.extendStick] = new TutorialInput("extendStick", actions.ThrustUp, 0);
            tutorialInputs[InputLibrary.thrustUp] = new TutorialInput("thrustUp", actions.ThrustUp, 4);
            tutorialInputs[InputLibrary.thrustDown] = new TutorialInput("thrustDown", actions.ThrustDown, 5);

            tutorialInputs[InputLibrary.rollMode] = new TutorialInput("rollMode", actions.RollMode, 8);

            tutorialInputs[InputLibrary.probeReverse] = new TutorialInput("probeReverse", actions.RollMode, 8);

            tutorialInputs[InputLibrary.cancel] = new TutorialInput("back", actions.Back, 9);

            // Show these right away instead of waiting for a prompt.
            var move = new TutorialInput("move", actions.Move, 6);
            var look = new TutorialInput("look", actions.Look, 7);
            AddToQueue(move);
            AddToQueue(look);
        }

        static void AddToQueue(TutorialInput input)
        {
            queue.Add(input);
            queue.Sort((a, b) => a.priority - b.priority);
        }

        void Update()
        {
            if (queue.Count > 0)
            {
                queue[0].Show();
            }
            if (!isShowingControlleModels && queue.Count > 0)
            {
                ShowControllerModels();
            }
            else if (isShowingControlleModels && queue.Count == 0)
            {
                HideControllerModels();
            }
        }

        void ShowControllerModels()
        {
            isShowingControlleModels = true;
            controllerModels.ForEach(model =>
            {
                model.enabled = true;
                model.gameObject.SetActive(true);
            });
        }

        void HideControllerModels()
        {
            isShowingControlleModels = false;
            controllerModels.ForEach(model =>
            {
                model.enabled = false;
                model.gameObject.SetActive(false);
            });
        }

        void CreateControllerModel(Transform hand)
        {
            var controllerModel = new GameObject().AddComponent<SteamVR_RenderModel>();
            controllerModel.updateDynamically = false;
            controllerModel.createComponents = false;
            controllerModel.transform.parent = hand;
            controllerModel.transform.localPosition = Vector3.zero;
            controllerModel.transform.localRotation = Quaternion.identity;

            controllerModels.Add(controllerModel);
        }

        internal static class Patches
        {
            public static void Patch()
            {
                NomaiVR.Post<ScreenPrompt>("SetVisibility", typeof(Patches), nameof(SetPromptVisibility));
            }

            static void SetPromptVisibility(bool isVisible, List<InputCommand> ____commandList)
            {
                foreach (var command in ____commandList)
                {
                    if (isVisible && tutorialInputs.ContainsKey(command))
                    {
                        var tutorialInput = tutorialInputs[command];
                        if (tutorialInput == null)
                        {
                            continue;
                        }

                        if (!tutorialInput.isDone && !queue.Contains(tutorialInput))
                        {
                            AddToQueue(tutorialInputs[command]);
                            queue.Sort((a, b) => a.priority - b.priority);
                        }
                    }
                }
            }
        }

        class TutorialInput
        {
            public bool isDone;
            public SteamVR_Action action;
            public bool isShowing;
            public int priority;
            string name;

            public TutorialInput(string name, SteamVR_Action action, int priority)
            {
                this.name = name;
                this.action = action;
                this.priority = priority;
                action.HideOrigins();

                if (NomaiVR.SaveFile.tutorialSteps.Contains(name))
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

                Timers.ExecuteAfter(() =>
                {
                    isShowing = false;
                    isDone = true;
                    queue.Remove(this);
                }, 500);
                NomaiVR.SaveFile.AddTutorialStep(name);
            }

            public void Show()
            {
                if (isDone)
                {
                    queue.Remove(this);
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
