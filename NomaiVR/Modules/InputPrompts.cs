using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class InputPrompts: MonoBehaviour {
        static TutorialState _tutorialState;

        void Start () {
            SetTutorialState(TutorialState.LOOK);


            SteamVR_Actions.default_Look.onChange += OnLookChange;
            SteamVR_Actions.default_Move.onChange += OnMoveChange;
            SteamVR_Actions.default_PrimaryAction.onChange += OnPrimaryChange;
        }

        void SetTutorialState (TutorialState state) {
            _tutorialState = state;

            if (state == TutorialState.LOOK) {
                SteamVR_Actions.default_Look.ShowOrigins();
            } else if (state == TutorialState.MOVE) {
                SteamVR_Actions.default_Move.ShowOrigins();
            } else if (state == TutorialState.INTERACT) {
                SteamVR_Actions.default_PrimaryAction.ShowOrigins();
            } else if (state == TutorialState.JUMP) {
                SteamVR_Actions.default_PrimaryAction.HideOrigins();
            }
        }

        void GoToMoveState () {
            SetTutorialState(TutorialState.MOVE);
        }

        void GoToInteractState () {
            SetTutorialState(TutorialState.INTERACT);
        }

        void GoToJumpState () {
            SetTutorialState(TutorialState.JUMP);
        }

        void OnLookChange (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            if (_tutorialState == TutorialState.LOOK && delta.magnitude > 0.1f) {
                Invoke(nameof(GoToMoveState), 3);
            }
        }

        void OnMoveChange (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            if (_tutorialState == TutorialState.MOVE && delta.magnitude > 0.1f) {
                Invoke(nameof(GoToInteractState), 3);
            }
        }

        private void OnPrimaryChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if (_tutorialState == TutorialState.INTERACT && newState) {
                Invoke(nameof(GoToJumpState), 3);
            }
        }

        void Update () {
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<JumpPromptTrigger>("OnTriggerEnter", typeof(Patches), nameof(ShowJumpPrompt));
                NomaiVR.Pre<JumpPromptTrigger>("OnTriggerExit", typeof(Patches), nameof(HideJumpPrompt));
                NomaiVR.Post<ProbePromptController>("LateInitialize", typeof(Patches), nameof(RemoveProbePrompts));
                NomaiVR.Post<ProbePromptController>("Awake", typeof(Patches), nameof(ChangeProbePrompts));
                NomaiVR.Post<SignalscopePromptController>("LateInitialize", typeof(Patches), nameof(RemoveSignalscopePrompts));
                NomaiVR.Post<ShipPromptController>("LateInitialize", typeof(Patches), nameof(RemoveShipPrompts));
                NomaiVR.Post<NomaiTranslatorProp>("LateInitialize", typeof(Patches), nameof(RemoveTranslatorPrompts));
                NomaiVR.Post<PlayerSpawner>("Awake", typeof(Patches), nameof(RemoveJoystickPrompts));
            }

            static void ShowJumpPrompt () {
                if (_tutorialState == TutorialState.JUMP) {
                    SteamVR_Actions.default_Jump.ShowOrigins();
                }
            }

            static void HideJumpPrompt () {
                SteamVR_Actions.default_Jump.HideOrigins();
            }

            static void RemoveJoystickPrompts (ref bool ____lookPromptAdded) {
                ____lookPromptAdded = true;
            }

            static void ChangeProbePrompts (
                ref ScreenPrompt ____launchPrompt,
                ref ScreenPrompt ____retrievePrompt,
                ref ScreenPrompt ____takeSnapshotPrompt,
                ref ScreenPrompt ____forwardCamPrompt
            ) {
                ____launchPrompt = new ScreenPrompt(InputLibrary.interact, ____launchPrompt.GetText());
                ____forwardCamPrompt = new ScreenPrompt(InputLibrary.interact, ____takeSnapshotPrompt.GetText());
                ____retrievePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>");
                ____takeSnapshotPrompt = new ScreenPrompt(InputLibrary.interact, ____takeSnapshotPrompt.GetText());
            }

            static void RemoveProbePrompts (
                ScreenPrompt ____unequipPrompt,
                ScreenPrompt ____aimPrompt,
                ScreenPrompt ____photoModePrompt,
                ScreenPrompt ____reverseCamPrompt,
                ScreenPrompt ____rotatePrompt,
                ScreenPrompt ____rotateCenterPrompt,
                ScreenPrompt ____launchModePrompt
            ) {
                var manager = Locator.GetPromptManager();
                //manager.RemoveScreenPrompt(____unequipPrompt);
                manager.RemoveScreenPrompt(____aimPrompt);
                manager.RemoveScreenPrompt(____photoModePrompt);
                manager.RemoveScreenPrompt(____reverseCamPrompt);
                manager.RemoveScreenPrompt(____rotatePrompt);
                manager.RemoveScreenPrompt(____rotateCenterPrompt);
                manager.RemoveScreenPrompt(____launchModePrompt);
            }

            static void RemoveSignalscopePrompts (
                ScreenPrompt ____unequipPrompt,
                ScreenPrompt ____changeFrequencyPrompt,
                ScreenPrompt ____zoomLevelPrompt
            ) {
                var manager = Locator.GetPromptManager();
                //manager.RemoveScreenPrompt(____unequipPrompt);
                manager.RemoveScreenPrompt(____changeFrequencyPrompt);
                manager.RemoveScreenPrompt(____zoomLevelPrompt);
            }

            static void RemoveShipPrompts (
                ScreenPrompt ____freeLookPrompt,
                ScreenPrompt ____landingModePrompt,
                ScreenPrompt ____liftoffCamera
            ) {
                var manager = Locator.GetPromptManager();
                manager.RemoveScreenPrompt(____freeLookPrompt);
                manager.RemoveScreenPrompt(____landingModePrompt);
                manager.RemoveScreenPrompt(____liftoffCamera);
            }
            static void RemoveTranslatorPrompts (
                ScreenPrompt ____scrollPrompt,
                ScreenPrompt ____pagePrompt
            ) {
                var manager = Locator.GetPromptManager();
                manager.RemoveScreenPrompt(____scrollPrompt);
                manager.RemoveScreenPrompt(____pagePrompt);
            }
        }

        enum TutorialState {
            LOOK,
            MOVE,
            INTERACT,
            JUMP,
        }
    }
}
