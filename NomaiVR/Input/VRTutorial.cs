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
            tutorialInputs[InputLibrary.interact] = new TutorialInput(actions.PrimaryAction);
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
                if (vrAction is SteamVR_Action_Vector2) {
                    ((SteamVR_Action_Vector2) vrAction).onChange += OnChange;
                }
                if (vrAction is SteamVR_Action_Single) {
                    ((SteamVR_Action_Single) vrAction).onChange += OnChange;
                }
                if (vrAction is SteamVR_Action_Boolean) {
                    ((SteamVR_Action_Boolean) vrAction).onChange += OnChange;
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
            }

            public void Hide () {
                action.HideOrigins();
            }

            public void Show () {
                action.ShowOrigins();
            }
        }
    }
}
