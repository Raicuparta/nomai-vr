using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class VRTutorial: MonoBehaviour {
        static Dictionary<InputCommand, TutorialInput> tutorialInputs;
        static List<TutorialInput> queue;
        List<SteamVR_RenderModel> controllerModels;
        bool isShowingControlleModels;

        void Start () {
            NomaiVR.Log("Start VRTutorial");

            queue = new List<TutorialInput>();
            controllerModels = new List<SteamVR_RenderModel>();

            CreateControllerModel(Hands.RightHand);
            CreateControllerModel(Hands.LeftHand);
            controllerModels.ForEach(model => model.enabled = false);

            var actions = SteamVR_Actions._default;
            tutorialInputs = new Dictionary<InputCommand, TutorialInput>();
            var interact = new TutorialInput(actions.PrimaryAction, 0);
            tutorialInputs[InputLibrary.interact] = interact;
            tutorialInputs[InputLibrary.swapShipLogMode] = interact;
            tutorialInputs[InputLibrary.sleep] = interact;
            tutorialInputs[InputLibrary.suitMenu] = interact;
            tutorialInputs[InputLibrary.translate] = interact;
            tutorialInputs[InputLibrary.scopeView] = interact;
            tutorialInputs[InputLibrary.probeForward] = interact;
            tutorialInputs[InputLibrary.probeRetrieve] = interact;
            tutorialInputs[InputLibrary.lockOn] = interact;
            tutorialInputs[InputLibrary.autopilot] = interact;

            var jump = new TutorialInput(actions.Jump, 1);
            tutorialInputs[InputLibrary.jump] = jump;
            tutorialInputs[InputLibrary.boost] = jump;
            tutorialInputs[InputLibrary.markEntryOnHUD] = jump;
            tutorialInputs[InputLibrary.matchVelocity] = jump;

            var map = new TutorialInput(actions.Map, 3);
            tutorialInputs[InputLibrary.map] = map;

            var move = new TutorialInput(actions.Move, 6);
            tutorialInputs[InputLibrary.moveXZ] = move;
            tutorialInputs[InputLibrary.thrustX] = move;
            tutorialInputs[InputLibrary.thrustZ] = move;

            var look = new TutorialInput(actions.Look, 7);
            tutorialInputs[InputLibrary.look] = look;
            tutorialInputs[InputLibrary.yaw] = look;
            tutorialInputs[InputLibrary.pitch] = look;

            var thrustUp = new TutorialInput(actions.ThrottleUp, 4);
            tutorialInputs[InputLibrary.thrustUp] = thrustUp;
            tutorialInputs[InputLibrary.extendStick] = thrustUp;

            var thrustDown = new TutorialInput(actions.ThrottleDown, 5);
            tutorialInputs[InputLibrary.thrustDown] = thrustDown;

            var rollMode = new TutorialInput(actions.SecondaryAction, 8);
            tutorialInputs[InputLibrary.probeReverse] = rollMode;
            tutorialInputs[InputLibrary.rollMode] = rollMode;

            var back = new TutorialInput(actions.Back, 9);
            tutorialInputs[InputLibrary.cancel] = back;

        }

        void Update () {
            if (queue.Count > 0) {
                queue[0].Show();
            }
            if (!isShowingControlleModels && queue.Count > 0) {
                ShowControllerModels();
            } else if (isShowingControlleModels && queue.Count == 0) {
                HideControllerModels();
            }
        }

        void ShowControllerModels () {
            isShowingControlleModels = true;
            controllerModels.ForEach(model => {
                model.enabled = true;
                model.gameObject.SetActive(true);
            });
        }

        void HideControllerModels () {
            isShowingControlleModels = false;
            controllerModels.ForEach(model => {
                model.enabled = false;
                model.gameObject.SetActive(false);
            });
        }

        void CreateControllerModel (Transform hand) {
            var controllerModel = new GameObject().AddComponent<SteamVR_RenderModel>();
            controllerModel.updateDynamically = true;
            controllerModel.createComponents = true;
            controllerModel.transform.parent = hand;
            controllerModel.transform.localPosition = Vector3.zero;
            controllerModel.transform.localRotation = Quaternion.identity;

            controllerModels.Add(controllerModel);
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ScreenPrompt>("SetVisibility", typeof(Patches), nameof(SetPromptVisibility));
            }

            static void SetPromptVisibility (bool isVisible, List<InputCommand> ____commandList) {
                foreach (var command in ____commandList) {
                    if (isVisible && tutorialInputs.ContainsKey(command)) {
                        var tutorialInput = tutorialInputs[command];

                        if (!tutorialInput.isDone && !queue.Contains(tutorialInput)) {
                            queue.Add(tutorialInputs[command]);
                            queue.Sort((a, b) => a.priority - b.priority);
                        }

                    }
                }
            }
        }

        class TutorialInput {
            public bool isDone;
            public SteamVR_Action action;
            public bool isShowing;
            public int priority;

            public TutorialInput (SteamVR_Action action, int priority) {
                this.action = action;
                this.priority = priority;
                action.HideOrigins();

                if (action is SteamVR_Action_Vector2) {
                    ((SteamVR_Action_Vector2) action).onChange += OnChange;
                }
                if (action is SteamVR_Action_Single) {
                    ((SteamVR_Action_Single) action).onChange += OnChange;
                }
                if (action is SteamVR_Action_Boolean) {
                    ((SteamVR_Action_Boolean) action).onChange += OnChange;
                }
            }

            private void OnChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
                OnChange();
            }

            private void OnChange (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) {
                OnChange();
            }

            private void OnChange (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
                OnChange();
            }

            private void OnChange () {
                if (isShowing) {
                    Hide();
                    isDone = true;
                }
            }

            public void Hide () {
                isShowing = false;
                action.HideOrigins();
                queue.Remove(this);
            }

            public void Show () {
                if (isDone || isShowing) {
                    return;
                }
                isShowing = true;
                action.ShowOrigins();
            }
        }
    }
}
