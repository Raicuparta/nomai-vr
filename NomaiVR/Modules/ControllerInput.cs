using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class ControllerInput: MonoBehaviour {
        static Dictionary<XboxButton, float> _buttons;
        static Dictionary<SingleAxis, float> _singleAxes;
        static Dictionary<DoubleAxis, Vector2> _doubleAxes;
        public static bool IsGripping;

        void Awake () {
            OpenVR.Input.SetActionManifestPath(NomaiVR.Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
        }

        void Start () {
            NomaiVR.Log("Started ControllerInput");

            _buttons = new Dictionary<XboxButton, float>();
            _singleAxes = new Dictionary<SingleAxis, float>();
            _doubleAxes = new Dictionary<DoubleAxis, Vector2>();

            SteamVR_Actions.default_A.onChange += CreateButtonHandler(XboxButton.A);
            SteamVR_Actions.default_B.onChange += CreateButtonHandler(XboxButton.B);
            SteamVR_Actions.default_X.onChange += CreateButtonHandler(XboxButton.X);
            SteamVR_Actions.default_Y.onChange += OnYChange;

            SteamVR_Actions.default_DUp.onChange += CreateButtonHandler(XboxAxis.dPadY, 1);
            SteamVR_Actions.default_DDown.onChange += CreateButtonHandler(XboxAxis.dPadY, -1);
            SteamVR_Actions.default_DLeft.onChange += CreateButtonHandler(XboxAxis.dPadX, -1);
            SteamVR_Actions.default_DRight.onChange += CreateButtonHandler(XboxAxis.dPadX, 1);

            SteamVR_Actions.default_LB.onChange += OnLBChange;
            SteamVR_Actions.default_LB.onChange += CreateButtonHandler(XboxButton.LeftBumper);
            SteamVR_Actions.default_RB.onChange += onRBChange;

            SteamVR_Actions.default_LT.onChange += CreateSingleAxisHandler(XboxAxis.leftTrigger);
            SteamVR_Actions.default_RT.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);

            SteamVR_Actions.default_Start.onChange += CreateButtonHandler(XboxButton.Start);
            SteamVR_Actions.default_Select.onChange += CreateButtonHandler(XboxButton.Select);

            SteamVR_Actions.default_LClick.onChange += CreateButtonHandler(XboxButton.LeftStickClick);
            SteamVR_Actions.default_RClick.onChange += CreateButtonHandler(XboxButton.RightStickClick);

            SteamVR_Actions.default_LStick.onChange += CreateDoubleAxisHandler(XboxAxis.leftStick, XboxAxis.leftStickX, XboxAxis.leftStickY);
            SteamVR_Actions.default_RStick.onChange += CreateDoubleAxisHandler(XboxAxis.rightStick, XboxAxis.rightStickX, XboxAxis.rightStickY);

            NomaiVR.Helper.HarmonyHelper.AddPrefix<SingleAxisCommand>("Update", typeof(Patches), "SingleAxisUpdate");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<DoubleAxisCommand>("Update", typeof(Patches), "DoubleAxisUpdate");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<OWInput>("Update", typeof(Patches), "OWInputUpdate");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<Campfire>("Awake", typeof(Patches), "CampfireAwake");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<SingleInteractionVolume>("ChangePrompt", typeof(Patches), "InteractionVolumeChangePrompt");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<SingleInteractionVolume>("Awake", typeof(Patches), "InteractionVolumeAwake");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<SingleInteractionVolume>("SetKeyCommandVisible", typeof(Patches), "InteractionVolumeVisible");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<MultipleInteractionVolume>("AddInteraction", typeof(Patches), "MultipleInteractionAdd");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<MultipleInteractionVolume>("SetKeyCommandVisible", typeof(Patches), "MultipleInteractionAdd");
        }

        private void OnYChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            var value = newState ? 1 : 0;
            if (OWInput.IsInputMode(InputMode.ShipCockpit)) {
                _buttons[XboxButton.Y] = value;
            } else {
                _buttons[XboxButton.Start] = value;
            }
        }

        void onRBChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            IsGripping = newState;
            float value = newState ? 1 : 0;

            bool isInteractMode = Common.ToolSwapper.IsInToolMode(ToolMode.None) || Common.ToolSwapper.IsInToolMode(ToolMode.Item);

            if (Common.ToolSwapper.IsInToolMode(ToolMode.SignalScope)) {
                _singleAxes[XboxAxis.dPadX] = value;
            } else if (!isInteractMode || OWInput.IsInputMode(InputMode.ShipCockpit)) {
                _buttons[XboxButton.RightBumper] = value;
            }
            if (Common.ToolSwapper.IsInToolMode(ToolMode.Translator)) {
                _singleAxes[XboxAxis.dPadX] = value;
            }
            if (isInteractMode && !OWInput.IsInputMode(InputMode.ShipCockpit)) {
                _buttons[XboxButton.X] = value;
            }
        }

        public static void ResetRB () {
            _buttons[XboxButton.RightBumper] = 0;
        }

        public static void SimulateButton (XboxButton button, float value) {
            _buttons[button] = value;
        }

        private void OnLBChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if (OWInput.IsInputMode(InputMode.ShipCockpit)) {
                _singleAxes[XboxAxis.dPadY] = newState ? 1 : 0;
            }
        }

        SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler (SingleAxis singleAxis, int axisDirection) {
            return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) => {
                _singleAxes[singleAxis] = axisDirection * Mathf.Round(newAxis * 10) / 10;
            };
        }

        SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler (SingleAxis singleAxis) {
            return CreateSingleAxisHandler(singleAxis, 1);
        }

        SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler (SingleAxis singleAxis, int axisDirection) {
            return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) => {
                _singleAxes[singleAxis] = axisDirection * (newState ? 1 : 0);
            };
        }

        SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler (XboxButton button) {
            return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) => {
                _buttons[button] = newState ? 1 : 0;
            };
        }

        SteamVR_Action_Vector2.ChangeHandler CreateDoubleAxisHandler (DoubleAxis doubleAxis, SingleAxis singleX, SingleAxis singleY) {
            return (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) => {
                var x = Mathf.Round(axis.x * 100) / 100;
                var y = Mathf.Round(axis.y * 100) / 100;
                _doubleAxes[doubleAxis] = new Vector2(x, y);
                _singleAxes[singleX] = x;
                _singleAxes[singleY] = y;
            };
        }

        static void SetInputValues (object inputValue, params string[] inputActions) {
            foreach (string action in inputActions) {
                var actionField = typeof(InputLibrary).GetAnyField(action);
                var actionValue = actionField.GetValue(null);

                var isSingleAxis = actionValue.GetType() == typeof(SingleAxisCommand);

                actionValue.SetValue("_value", inputValue);
            }
        }

        internal static class Patches {
            static bool DoubleAxisUpdate (ref Vector2 ____value, DoubleAxis ____gamepadAxis) {
                if (____gamepadAxis != null && _doubleAxes.ContainsKey(____gamepadAxis)) {
                    ____value = _doubleAxes[____gamepadAxis];
                }

                return false;
            }

            static bool SingleAxisUpdate (
                SingleAxisCommand __instance,
                XboxButton ____xboxButtonPositive,
                XboxButton ____xboxButtonNegative,
                SingleAxis ____gamepadAxisPositive,
                SingleAxis ____gamepadAxisNegative,
                ref float ____value,
                ref bool ____newlyPressedThisFrame,
                ref float ____lastValue,
                ref float ____lastPressedDuration,
                ref float ____pressedDuration,
                ref float ____realtimeSinceLastUpdate,
                int ____axisDirection
            ) {
                ____newlyPressedThisFrame = false;
                ____lastValue = ____value;
                ____value = 0f;

                if (____gamepadAxisPositive != null && _singleAxes.ContainsKey(____gamepadAxisPositive)) {
                    ____value += _singleAxes[____gamepadAxisPositive] * (float) ____axisDirection;

                    if (____gamepadAxisNegative != null && _singleAxes.ContainsKey(____gamepadAxisNegative)) {
                        ____value -= _singleAxes[____gamepadAxisNegative] * (float) ____axisDirection;
                    }
                } else {
                    if (_buttons.ContainsKey(____xboxButtonPositive)) {
                        ____value += _buttons[____xboxButtonPositive];
                    }

                    if (_buttons.ContainsKey(____xboxButtonNegative)) {
                        ____value -= _buttons[____xboxButtonNegative];
                    }
                }

                ____lastPressedDuration = ____pressedDuration;
                ____pressedDuration = ((!__instance.IsPressed()) ? 0f : (____pressedDuration + (Time.realtimeSinceStartup - ____realtimeSinceLastUpdate)));
                ____realtimeSinceLastUpdate = Time.realtimeSinceStartup;

                return false;
            }

            static void OWInputUpdate (ref bool ____usingGamepad) {
                ____usingGamepad = true;
            }

            static void CampfireAwake (
                SingleInteractionVolume ____interactVolume,
                bool ____canSleepHere,
                ref ScreenPrompt ____sleepPrompt,
                ref ScreenPrompt ____wakePrompt,
                Campfire __instance
            ) {
                if (____interactVolume != null && ____canSleepHere) {
                    ____sleepPrompt = new ScreenPrompt(UITextLibrary.GetString(UITextType.CampfireDozeOff), 0);
                    ____interactVolume.SetValue("_textID", UITextType.None);
                    ____interactVolume.SetValue("_usingPromptWithCommand", false);
                    ____interactVolume.SetValue("OnPressInteract", null);
                    ____interactVolume.OnPressInteract += () => __instance.Invoke("StartSleeping");
                }
            }

            static bool InteractionVolumeChangePrompt (UITextType promptID, ref bool ____usingPromptWithCommand) {
                if (promptID == UITextType.RoastingPrompt) {
                    return false;
                }

                return true;
            }

            static void InteractionVolumeAwake (ref bool ____usingPromptWithCommand, SingleInteractionVolume __instance) {
                ____usingPromptWithCommand = false;
            }

            static bool InteractionVolumeVisible (ref bool ____usingPromptWithCommand, SingleInteractionVolume __instance) {
                ____usingPromptWithCommand = false;
                __instance.Invoke("UpdatePromptVisibility");
                return false;
            }

            static void MultipleInteractionAdd (List<MultipleInteractionVolume.Interaction> ____listInteractions) {
                foreach (var interaction in ____listInteractions) {
                    if (interaction.inputCommand == InputLibrary.interact) {
                        interaction.displayPromptCommandIcon = false;
                    }
                }
            }
        }
    }
}
