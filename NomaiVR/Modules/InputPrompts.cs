using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class InputPrompts: MonoBehaviour {
        TutorialState _tutorialState;

        void Start () {
            SetTutorialState(TutorialState.LOOK);
        }

        void SetTutorialState (TutorialState state) {
            _tutorialState = state;

            if (state == TutorialState.LOOK) {
                SteamVR_Actions.default_Look.ShowOrigins();
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<JumpPromptTrigger>("OnTriggerEnter", typeof(Patches), nameof(ShowJumpPrompt));
                NomaiVR.Pre<JumpPromptTrigger>("OnTriggerExit", typeof(Patches), nameof(HideJumpPrompt));
            }

            static void ShowJumpPrompt () {
                SteamVR_Actions.default_Jump.ShowOrigins();
            }

            static void HideJumpPrompt () {
                SteamVR_Actions.default_Jump.HideOrigins();
            }
        }

        enum TutorialState {
            LOOK,
            MOVE
        }
    }
}
