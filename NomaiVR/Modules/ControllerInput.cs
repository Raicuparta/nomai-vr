using OWML.ModHelper.Events;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class ControllerInput: MonoBehaviour {
        static Dictionary<XboxButton, float> _buttons;
        static Dictionary<string, float> _singleAxes;
        static Dictionary<DoubleAxis, Vector2> _doubleAxes;
        public static bool IsGripping { get; private set; }

        void Awake () {
            OpenVR.Input.SetActionManifestPath(NomaiVR.Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
        }

        void Start () {
            NomaiVR.Log("Started ControllerInput");

            _buttons = new Dictionary<XboxButton, float>();
            _singleAxes = new Dictionary<string, float>();
            _doubleAxes = new Dictionary<DoubleAxis, Vector2>();

            SteamVR_Actions.default_Jump.onChange += CreateButtonHandler(XboxButton.A);
            SteamVR_Actions.default_Back.onChange += OnBackChange;
            SteamVR_Actions.default_PrimaryAction.onChange += OnPrimaryActionChange;

            SteamVR_Actions.default_Menu.onChange += CreateButtonHandler(XboxButton.Start);
            SteamVR_Actions.default_Map.onChange += CreateButtonHandler(XboxButton.Select);

            SteamVR_Actions.default_SecondaryAction.onChange += OnSecondaryActionChange;
            SteamVR_Actions.default_SecondaryAction.onChange += CreateButtonHandler(XboxAxis.dPadX, -1);
            SteamVR_Actions.default_Grip.onChange += OnGripChange;

            SteamVR_Actions.default_ThrottleDown.onChange += CreateSingleAxisHandler(XboxAxis.leftTrigger);
            SteamVR_Actions.default_ThrottleUp.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);
            SteamVR_Actions.default_ThrottleUp.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);

            SteamVR_Actions.default_Move.onChange += CreateDoubleAxisHandler(XboxAxis.leftStick, XboxAxis.leftStickX, XboxAxis.leftStickY);
            SteamVR_Actions.default_Look.onChange += CreateDoubleAxisHandler(XboxAxis.rightStick, XboxAxis.rightStickX, XboxAxis.rightStickY);

            NomaiVR.Helper.HarmonyHelper.AddPrefix<SingleAxisCommand>("Update", typeof(Patches), "SingleAxisUpdate");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<OWInput>("Update", typeof(Patches), "OWInputUpdate");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<Campfire>("Awake", typeof(Patches), "CampfireAwake");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<SingleInteractionVolume>("ChangePrompt", typeof(Patches), "InteractionVolumeChangePrompt");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<SingleInteractionVolume>("Awake", typeof(Patches), "InteractionVolumeAwake");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<SingleInteractionVolume>("SetKeyCommandVisible", typeof(Patches), "InteractionVolumeVisible");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<MultipleInteractionVolume>("AddInteraction", typeof(Patches), "MultipleInteractionAdd");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<MultipleInteractionVolume>("SetKeyCommandVisible", typeof(Patches), "MultipleInteractionAdd");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<ItemTool>("Start", typeof(Patches), "ItemToolStart");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<OWInput>("Awake", typeof(Patches), "EnableListenForAllJoysticks");
            NomaiVR.Helper.HarmonyHelper.AddPostfix<PadEZ.PadManager>("GetAxis", typeof(Patches), "GetAxis");
        }

        private void OnBackChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if (!IsGripping) {
                _buttons[XboxButton.B] = newState ? 1 : 0;
            }
        }

        private void OnGripChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            IsGripping = newState;
        }

        private void OnPrimaryActionChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            var value = newState ? 1 : 0;

            switch (Common.ToolSwapper.GetToolMode()) {
                case ToolMode.SignalScope:
                    _singleAxes[XboxAxis.dPadX.GetInputAxisName(0)] = value;
                    break;
                case ToolMode.Translator:
                    _singleAxes[XboxAxis.dPadX.GetInputAxisName(0)] = value;
                    _buttons[XboxButton.RightBumper] = value;
                    break;
                case ToolMode.Probe:
                    _buttons[XboxButton.RightBumper] = value;
                    break;
                default:
                    _buttons[XboxButton.X] = value;
                    break;
            }

            if (Common.ToolSwapper.GetToolGroup() == ToolGroup.Ship) {
                if (!newState) {
                    _buttons[XboxButton.X] = value;
                }
            }
        }

        private void OnSecondaryActionChange (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if (!Common.ToolSwapper.IsInToolMode(ToolMode.Probe)) {
                _buttons[XboxButton.LeftBumper] = newState ? 1 : 0;
            }
        }

        public static void SimulateInput (XboxButton button, float value) {
            _buttons[button] = value;
        }

        public static void SimulateInput (SingleAxis axis, float value) {
            _singleAxes[axis.GetInputAxisName(0)] = value;
        }

        SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler (SingleAxis singleAxis, int axisDirection) {
            return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) => {
                _singleAxes[singleAxis.GetInputAxisName(0)] = axisDirection * Mathf.Round(newAxis * 10) / 10;
            };
        }

        SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler (SingleAxis singleAxis) {
            return CreateSingleAxisHandler(singleAxis, 1);
        }

        SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler (SingleAxis singleAxis, int axisDirection) {
            return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) => {
                _singleAxes[singleAxis.GetInputAxisName(0)] = axisDirection * (newState ? 1 : 0);
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
                _singleAxes[singleX.GetInputAxisName(0)] = x;
                _singleAxes[singleY.GetInputAxisName(0)] = y;
            };
        }

        internal static class Patches {
            static float GetAxis (float __result, string axisName) {
                if (_singleAxes.ContainsKey(axisName)) {
                    return _singleAxes[axisName];
                }
                return __result;
            }

            static bool SingleAxisUpdate (
                SingleAxisCommand __instance,
                XboxButton ____xboxButtonPositive,
                XboxButton ____xboxButtonNegative,
                ref float ____value,
                ref bool ____newlyPressedThisFrame,
                ref float ____lastValue,
                ref float ____lastPressedDuration,
                ref float ____pressedDuration,
                ref float ____realtimeSinceLastUpdate
            ) {
                if (____xboxButtonPositive == XboxButton.None && ____xboxButtonNegative == XboxButton.None) {
                    return true;
                }

                ____newlyPressedThisFrame = false;
                ____lastValue = ____value;
                ____value = 0f;


                if (_buttons.ContainsKey(____xboxButtonPositive)) {
                    ____value += _buttons[____xboxButtonPositive];
                }

                if (_buttons.ContainsKey(____xboxButtonNegative)) {
                    ____value -= _buttons[____xboxButtonNegative];
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

            static void ItemToolStart (ref ScreenPrompt ____interactButtonPrompt) {
                Locator.GetPromptManager().RemoveScreenPrompt(____interactButtonPrompt);
                ____interactButtonPrompt = new ScreenPrompt(string.Empty, 0);
                Locator.GetPromptManager().AddScreenPrompt(____interactButtonPrompt);
            }

            static void EnableListenForAllJoysticks () {
                InputLibrary.landingCamera.ChangeBinding(XboxButton.DPadDown, KeyCode.None);
            }
        }
    }
}
