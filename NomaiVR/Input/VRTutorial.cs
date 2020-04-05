using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class VRTutorial: MonoBehaviour {
        static Dictionary<InputCommand, TutorialInput> tutorialInputs;

        void Start () {
            NomaiVR.Log("Start VRTutorial");

            var actions = SteamVR_Actions._default;
            tutorialInputs = new Dictionary<InputCommand, TutorialInput>();
            var interact = new TutorialInput(actions.PrimaryAction);
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

            var jump = new TutorialInput(actions.Jump);
            tutorialInputs[InputLibrary.jump] = jump;
            tutorialInputs[InputLibrary.boost] = jump;
            tutorialInputs[InputLibrary.markEntryOnHUD] = jump;
            tutorialInputs[InputLibrary.matchVelocity] = jump;

            var map = new TutorialInput(actions.Map);
            tutorialInputs[InputLibrary.map] = map;

            var move = new TutorialInput(actions.Move);
            tutorialInputs[InputLibrary.moveXZ] = move;
            tutorialInputs[InputLibrary.thrustX] = move;
            tutorialInputs[InputLibrary.thrustZ] = move;

            var look = new TutorialInput(actions.Look);
            tutorialInputs[InputLibrary.look] = look;
            tutorialInputs[InputLibrary.yaw] = look;
            tutorialInputs[InputLibrary.pitch] = look;

            var thrustUp = new TutorialInput(actions.ThrottleUp);
            tutorialInputs[InputLibrary.thrustUp] = thrustUp;
            tutorialInputs[InputLibrary.extendStick] = thrustUp;

            var thrustDown = new TutorialInput(actions.ThrottleDown);
            tutorialInputs[InputLibrary.thrustDown] = thrustDown;

            var rollMode = new TutorialInput(actions.SecondaryAction);
            tutorialInputs[InputLibrary.probeReverse] = rollMode;
            tutorialInputs[InputLibrary.rollMode] = rollMode;

        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ScreenPrompt>("SetVisibility", typeof(Patches), nameof(SetPromptVisibility));
            }

            static void SetPromptVisibility (bool isVisible, List<InputCommand> ____commandList) {
                foreach (var command in ____commandList) {
                    if (isVisible && tutorialInputs.ContainsKey(command)) {
                        tutorialInputs[command].Show();
                    }
                }
            }
        }

        class TutorialInput {
            public bool isDone;
            public SteamVR_Action action;

            public TutorialInput (SteamVR_Action vrAction) {
                action = vrAction;
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
                Hide();
                isDone = true;
            }

            public void Hide () {
                action.HideOrigins();
            }

            public void Show () {
                if (isDone) {
                    return;
                }
                action.ShowOrigins();
            }
        }
    }
}
