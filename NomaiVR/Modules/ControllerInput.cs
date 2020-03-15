using OWML.ModHelper.Events;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR {
    class ControllerInput: MonoBehaviour {
        static ControllerInput _instance;
        static Dictionary<XboxButton, float> _buttons;
        static Dictionary<string, float> _singleAxes;
        static Dictionary<DoubleAxis, Vector2> _doubleAxes;
        static PlayerResources _playerResources;
        public static bool IsGripping { get; private set; }
        float _primaryLastTime = -1;
        const float holdDuration = 0.3f;
        bool _justHeld;
        ScreenPrompt _repairPrompt;

        void Awake () {
            OpenVR.Input.SetActionManifestPath(NomaiVR.Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
        }

        void Start () {
            NomaiVR.Log("Started ControllerInput");

            _instance = this;
            _buttons = new Dictionary<XboxButton, float>();
            _singleAxes = new Dictionary<string, float>();
            _doubleAxes = new Dictionary<DoubleAxis, Vector2>();

            SteamVR_Actions.default_Jump.onChange += CreateButtonHandler(XboxButton.A);
            SteamVR_Actions.default_Back.onChange += OnBackChange;
            SteamVR_Actions.default_PrimaryAction.onChange += OnPrimaryActionChange;
            SteamVR_Actions.default_SecondaryAction.onChange += CreateButtonHandler(XboxButton.LeftBumper);
            SteamVR_Actions.default_Grip.onChange += OnGripChange;

            SteamVR_Actions.default_Menu.onChange += CreateButtonHandler(XboxButton.Start);
            SteamVR_Actions.default_Map.onChange += CreateButtonHandler(XboxButton.Select);

            SteamVR_Actions.default_ThrottleDown.onChange += CreateSingleAxisHandler(XboxAxis.leftTrigger);
            SteamVR_Actions.default_ThrottleUp.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);
            SteamVR_Actions.default_ThrottleUp.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);

            SteamVR_Actions.default_Move.onChange += CreateDoubleAxisHandler(XboxAxis.leftStick, XboxAxis.leftStickX, XboxAxis.leftStickY);
            SteamVR_Actions.default_Look.onChange += CreateDoubleAxisHandler(XboxAxis.rightStick, XboxAxis.rightStickX, XboxAxis.rightStickY);

            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
        }

        void OnWakeUp () {
            _repairPrompt = FindObjectOfType<FirstPersonManipulator>().GetValue<ScreenPrompt>("_repairScreenPrompt");
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

            var isInShip = Common.ToolSwapper.GetToolGroup() == ToolGroup.Ship;

            if (Common.ToolSwapper.IsInToolMode(ToolMode.None) || Common.ToolSwapper.IsInToolMode(ToolMode.Item)) {
                var canRepairSuit = _playerResources.IsSuitPunctured() && OWInput.IsInputMode(InputMode.Character) && !Locator.GetToolModeSwapper().IsSuitPatchingBlocked();

                if (_repairPrompt != null && !_repairPrompt.IsVisible() && Common.ToolSwapper.GetToolGroup() != ToolGroup.Ship && !canRepairSuit) {
                    if (newState) {
                        _primaryLastTime = fromAction.changedTime;
                    } else {
                        _primaryLastTime = -1;
                        if (!_justHeld) {
                            SimulateInput(XboxButton.X);
                        }
                        _justHeld = false;
                    }
                } else {
                    _buttons[XboxButton.X] = value;
                }
            } else if (!isInShip || Common.ToolSwapper.IsInToolMode(ToolMode.Probe)) {
                _buttons[XboxButton.RightBumper] = value;
            } else if (Common.ToolSwapper.IsInToolMode(ToolMode.SignalScope)) {
                _singleAxes[XboxAxis.dPadX.GetInputAxisName(0)] = value;
            }

            if (isInShip) {
                if (!newState) {
                    _buttons[XboxButton.X] = value;
                }
            }
        }

        static IEnumerator<WaitForSecondsRealtime> ResetInput (XboxButton button) {
            yield return new WaitForSecondsRealtime(0.1f);
            SimulateInput(button, 0);
        }

        public static void SimulateInput (XboxButton button) {
            _buttons[button] = 1;
            _instance.StartCoroutine(ResetInput(button));
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

        void Update () {
            if ((_primaryLastTime != -1) && (Time.realtimeSinceStartup - _primaryLastTime > holdDuration)) {
                SimulateInput(XboxButton.Y);
                _primaryLastTime = -1;
                _justHeld = true;
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<SingleAxisCommand>("Update", typeof(Patches), nameof(Patches.SingleAxisUpdate));
                NomaiVR.Pre<OWInput>("UpdateActiveInputDevice", typeof(Patches), nameof(Patches.OWInputUpdate));
                NomaiVR.Post<ItemTool>("Start", typeof(Patches), nameof(Patches.ItemToolStart));
                NomaiVR.Pre<OWInput>("Awake", typeof(Patches), nameof(Patches.EnableListenForAllJoysticks));
                NomaiVR.Post<PadEZ.PadManager>("GetAxis", typeof(Patches), nameof(Patches.GetAxis));
                NomaiVR.Post<PlayerResources>("Awake", typeof(Patches), nameof(PlayerResourcesAwake));
            }

            static void PlayerResourcesAwake () {
                _playerResources = GameObject.FindObjectOfType<PlayerResources>();
            }

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

            static bool OWInputUpdate (ref bool ____usingGamepad) {
                ____usingGamepad = true;
                return false;
            }

            static void ItemToolStart (ref ScreenPrompt ____interactButtonPrompt) {
                Locator.GetPromptManager().RemoveScreenPrompt(____interactButtonPrompt);
                ____interactButtonPrompt = new ScreenPrompt(string.Empty, 0);
                Locator.GetPromptManager().AddScreenPrompt(____interactButtonPrompt);
            }

            static void EnableListenForAllJoysticks () {
                InputLibrary.landingCamera.ChangeBinding(XboxButton.DPadDown, KeyCode.None);
                InputLibrary.signalscope.ChangeBinding(XboxButton.DPadRight, KeyCode.None);
            }
        }
    }
}
