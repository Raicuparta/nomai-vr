using OWML.ModHelper.Events;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Valve.VR;
using System.Linq;

namespace NomaiVR
{
    internal class ControllerInput : NomaiVRModule<ControllerInput.Behaviour, ControllerInput.Behaviour.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;
        public static Dictionary<JoystickButton, VRActionInput> buttonActions;
        public static Dictionary<AxisIdentifier, VRActionInput> axisActions;
        public static VRActionInput[] otherActions;

        public class VRActionInput
        {
            public string Hand;
            public readonly string Source;
            public readonly string Color;
            public readonly List<string> Prefixes = new List<string>();

            public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false)
            {
                Hand = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_Hand });
                Source = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_InputSource });
                Color = color;

                if (isLongPress)
                {
                    Prefixes.Add("Long Press");
                }
            }

            public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false) : this(action, TextHelper.ORANGE, isLongPress) { }

            public string GetText()
            {
                var prefix = TextHelper.TextWithColor(string.Join(" ", Prefixes.ToArray()), TextHelper.ORANGE);
                return $"{prefix} {TextHelper.TextWithColor($"{Hand} {Source}", Color)}";
            }
        }

        public class Behaviour : MonoBehaviour
        {
            private static Behaviour _instance;
            private static Dictionary<JoystickButton, float> _buttons;
            private static Dictionary<string, float> _singleAxes;
            private static PlayerResources _playerResources;
            public static bool IsGripping { get; private set; }

            private float _primaryLastTime = -1;
            private const float holdDuration = 0.3f;
            private bool _justHeld;
            private ScreenPrompt _repairPrompt;

            internal void Awake()
            {
                OpenVR.Input.SetActionManifestPath(NomaiVR.Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
            }

            internal void Start()
            {
                _instance = this;
                _buttons = new Dictionary<JoystickButton, float>();
                _singleAxes = new Dictionary<string, float>();

                SetUpSteamVRActionHandlers();
                ReplaceInputs();
                Invoke(nameof(DoTheThings), 1);
                GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            }

            private bool HasAxisWithSameName(VRActionInput button)
            {
                foreach (KeyValuePair<AxisIdentifier, VRActionInput> axisEntry in axisActions)
                {
                    var axis = axisEntry.Value;
                    if (button.Hand == axis.Hand && button.Source == axis.Source)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool IsOppositeHandWithSameName(VRActionInput actionInputA, VRActionInput actionInputB)
            {
                if (actionInputA == actionInputB)
                {
                    return false;
                }
                if (actionInputA.Hand != actionInputB.Hand && actionInputA.Source == actionInputB.Source)
                {
                    return true;
                }
                return false;
            }

            private bool HasOppositeHandButtonWithSameName(VRActionInput actionInput)
            {
                foreach (KeyValuePair<JoystickButton, VRActionInput> buttonEntry in buttonActions)
                {
                    if (IsOppositeHandWithSameName(actionInput, buttonEntry.Value))
                    {
                        return true;
                    }
                }
                foreach (KeyValuePair<AxisIdentifier, VRActionInput> axisEntry in axisActions)
                {
                    if (IsOppositeHandWithSameName(actionInput, axisEntry.Value))
                    {
                        return true;
                    }
                }
                foreach (var otherAction in otherActions)
                {
                    if (IsOppositeHandWithSameName(actionInput, otherAction))
                    {
                        return true;
                    }
                }
                return false;
            }

            public void DoTheThings()
            {
                var actionSet = SteamVR_Actions._default;
                buttonActions = new Dictionary<JoystickButton, VRActionInput>
                {
                    [JoystickButton.FaceDown] = new VRActionInput(actionSet.Jump, TextHelper.GREEN),
                    [JoystickButton.FaceRight] = new VRActionInput(actionSet.Back, TextHelper.RED),
                    [JoystickButton.FaceLeft] = new VRActionInput(actionSet.Interact, TextHelper.BLUE),
                    [JoystickButton.RightBumper] = new VRActionInput(actionSet.Interact, TextHelper.BLUE),
                    [JoystickButton.FaceUp] = new VRActionInput(actionSet.Interact, TextHelper.BLUE, true),
                    [JoystickButton.LeftBumper] = new VRActionInput(actionSet.RollMode),
                    [JoystickButton.Start] = new VRActionInput(actionSet.Menu),
                    [JoystickButton.Select] = new VRActionInput(actionSet.Map)
                };

                axisActions = new Dictionary<AxisIdentifier, VRActionInput>
                {
                    [AxisIdentifier.CTRLR_LTRIGGER] = new VRActionInput(actionSet.ThrustDown),
                    [AxisIdentifier.CTRLR_RTRIGGER] = new VRActionInput(actionSet.ThrustUp),
                    [AxisIdentifier.CTRLR_LSTICK] = new VRActionInput(actionSet.Move),
                    [AxisIdentifier.CTRLR_LSTICKX] = new VRActionInput(actionSet.Move),
                    [AxisIdentifier.CTRLR_LSTICKY] = new VRActionInput(actionSet.Move),
                    [AxisIdentifier.CTRLR_RSTICK] = new VRActionInput(actionSet.Look),
                    [AxisIdentifier.CTRLR_RSTICKX] = new VRActionInput(actionSet.Look),
                    [AxisIdentifier.CTRLR_RSTICKY] = new VRActionInput(actionSet.Look)
                };

                otherActions = new VRActionInput[]
                {
                    new VRActionInput(actionSet.Grip)
                };

                foreach (KeyValuePair<JoystickButton, VRActionInput> buttonEntry in buttonActions)
                {
                    var button = buttonEntry.Value;
                    if (HasAxisWithSameName(button))
                    {
                        button.Prefixes.Add("Click");
                    }

                    if (!HasOppositeHandButtonWithSameName(button))
                    {
                        button.Hand = "";
                    }
                }
            }

            private void SetUpSteamVRActionHandlers()
            {
                SteamVR_Actions.default_Jump.onChange += CreateButtonHandler(JoystickButton.FaceDown);
                SteamVR_Actions.default_Back.onChange += OnBackChange;
                SteamVR_Actions.default_Interact.onChange += OnPrimaryActionChange;
                SteamVR_Actions.default_RoolMode.onChange += CreateButtonHandler(JoystickButton.LeftBumper);
                SteamVR_Actions.default_Grip.onChange += OnGripChange;
                SteamVR_Actions.default_Menu.onChange += CreateButtonHandler(JoystickButton.Start);
                SteamVR_Actions.default_Map.onChange += CreateButtonHandler(JoystickButton.Select);
                SteamVR_Actions.default_ThrustDown.onChange += CreateSingleAxisHandler(AxisIdentifier.CTRLR_LTRIGGER);
                SteamVR_Actions.default_ThrustUp.onChange += CreateSingleAxisHandler(AxisIdentifier.CTRLR_RTRIGGER);
                SteamVR_Actions.default_Move.onChange += CreateDoubleAxisHandler(AxisIdentifier.CTRLR_LSTICKX, AxisIdentifier.CTRLR_LSTICKY);
                SteamVR_Actions.default_Look.onChange += CreateDoubleAxisHandler(AxisIdentifier.CTRLR_RSTICKX, AxisIdentifier.CTRLR_RSTICKY);
            }

            private void OnWakeUp()
            {
                _repairPrompt = FindObjectOfType<FirstPersonManipulator>().GetValue<ScreenPrompt>("_repairScreenPrompt");
            }

            private void OnBackChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                if (!IsGripping)
                {
                    _buttons[JoystickButton.FaceRight] = newState ? 1 : 0;
                }
            }

            private void OnGripChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                IsGripping = newState;
            }

            private void OnPrimaryActionChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                var value = newState ? 1 : 0;

                if (!SceneHelper.IsInGame())
                {
                    _buttons[JoystickButton.FaceLeft] = value;
                    return;
                }

                var toolSwapper = ToolHelper.Swapper;
                var isInShip = toolSwapper.GetToolGroup() == ToolGroup.Ship;
                var isUsingSignalscope = toolSwapper.IsInToolMode(ToolMode.SignalScope);
                var isUsingProbeLauncher = toolSwapper.IsInToolMode(ToolMode.Probe);
                var isUsingFixedProbeTool = OWInput.IsInputMode(InputMode.StationaryProbeLauncher) || OWInput.IsInputMode(InputMode.SatelliteCam);

                if (!isUsingFixedProbeTool && !ToolHelper.IsUsingAnyTool())
                {
                    var isRepairPromptVisible = _repairPrompt != null && !_repairPrompt.IsVisible();
                    var canRepairSuit = _playerResources.IsSuitPunctured() && OWInput.IsInputMode(InputMode.Character) && !ToolHelper.Swapper.IsSuitPatchingBlocked();

                    if (isRepairPromptVisible && !isInShip && !canRepairSuit)
                    {
                        if (newState)
                        {
                            _primaryLastTime = fromAction.changedTime;
                        }
                        else
                        {
                            _primaryLastTime = -1;
                            if (!_justHeld)
                            {
                                SimulateInput(JoystickButton.FaceLeft);
                            }
                            _justHeld = false;
                        }
                    }
                    else
                    {
                        _buttons[JoystickButton.FaceLeft] = value;
                    }
                }
                else if (!isInShip || isUsingProbeLauncher || isUsingFixedProbeTool)
                {
                    _buttons[JoystickButton.RightBumper] = value;
                }
                else if (isUsingSignalscope)
                {
                    _singleAxes[XboxAxis.dPadX.GetInputAxisName(0)] = value;
                }

                if (isInShip)
                {
                    if (!newState)
                    {
                        _buttons[JoystickButton.FaceLeft] = value;
                    }
                }
            }

            private static IEnumerator<WaitForSecondsRealtime> ResetInput(JoystickButton button)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                SimulateInput(button, 0);
            }

            public static void SimulateInput(JoystickButton button)
            {
                _buttons[button] = 1;
                _instance.StartCoroutine(ResetInput(button));
            }

            public static void SimulateInput(JoystickButton button, float value)
            {
                _buttons[button] = value;
            }

            public static void SimulateInput(AxisIdentifier axis, float value)
            {
                _singleAxes[InputTranslator.GetAxisName(axis)] = value;
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(AxisIdentifier axis, int axisDirection = 1)
            {
                return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) =>
                {
                    var axisName = InputTranslator.GetAxisName(axis);
                    _singleAxes[axisName] = axisDirection * Mathf.Round(newAxis * 10) / 10;
                };
            }

            private static SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler(JoystickButton button)
            {
                return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) =>
                {
                    _buttons[button] = newState ? 1 : 0;
                };
            }

            private static SteamVR_Action_Vector2.ChangeHandler CreateDoubleAxisHandler(AxisIdentifier axisX, AxisIdentifier axisY)
            {
                return (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) =>
                {
                    var axisNameX = InputTranslator.GetAxisName(axisX);
                    var axisNameY = InputTranslator.GetAxisName(axisY);
                    var x = Mathf.Round(axis.x * 100) / 100;
                    var y = Mathf.Round(axis.y * 100) / 100;
                    _singleAxes[axisNameX] = x;
                    _singleAxes[axisNameY] = y;
                };
            }

            internal void Update()
            {
                if ((_primaryLastTime != -1) && (Time.realtimeSinceStartup - _primaryLastTime > holdDuration))
                {
                    SimulateInput(JoystickButton.FaceUp);
                    _primaryLastTime = -1;
                    _justHeld = true;
                }
            }

            private static void SetCommandButton(SingleAxisCommand command, JoystickButton button)
            {
                command.SetValue("_gamepadBinding", new InputBinding(button));
            }

            private static void ReplaceInputs()
            {
                SetCommandButton(InputLibrary.landingCamera, JoystickButton.DPadDown);
                SetCommandButton(InputLibrary.signalscope, JoystickButton.DPadRight);
                SetCommandButton(InputLibrary.tabL, JoystickButton.None);
                SetCommandButton(InputLibrary.tabR, JoystickButton.None);
                SetCommandButton(InputLibrary.setDefaults, JoystickButton.None);
                SetCommandButton(InputLibrary.confirm, JoystickButton.None);
                SetCommandButton(InputLibrary.confirm2, JoystickButton.None);
                SetCommandButton(InputLibrary.enter, JoystickButton.FaceLeft);
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Pre<SingleAxisCommand>("UpdateInputCommand", typeof(Patch), nameof(SingleAxisUpdate));
                    NomaiVR.Pre<OWInput>("UpdateActiveInputDevice", typeof(Patch), nameof(OWInputUpdate));
                    NomaiVR.Pre<OWInput>("Awake", typeof(Patch), nameof(PostEnableListenForAllJoysticks));
                    NomaiVR.Post<PadEZ.PadManager_OW>("GetAxis", typeof(Patch), nameof(GetAxis));
                    NomaiVR.Post<PlayerResources>("Awake", typeof(Patch), nameof(PlayerResourcesAwake));
                    NomaiVR.Post<PadEZ.PadManager_OW>("GetKey", typeof(Patch), nameof(ResetPadManagerKeyboard));
                    NomaiVR.Post<PadEZ.PadManager_OW>("GetKeyDown", typeof(Patch), nameof(ResetPadManagerKeyboard));
                    NomaiVR.Post<PadEZ.PadManager_OW>("GetKeyUp", typeof(Patch), nameof(ResetPadManagerKeyboard));
                    NomaiVR.Post<OWInput>("IsGamepadEnabled", typeof(Patch), nameof(PostIsGamepadEnabled));
                    NomaiVR.Post<PadEZ.PadManager_OW>("IsGamepadActive", typeof(Patch), nameof(PostIsGamepadEnabled));

                    NomaiVR.Pre<DoubleAxisCommand>("UpdateInputCommand", typeof(Patch), nameof(PreUpdateDoubleAxisCommand));

                    var rumbleMethod = typeof(RumbleManager).GetAnyMethod("Update");
                    NomaiVR.Helper.HarmonyHelper.AddPrefix(rumbleMethod, typeof(Patch), nameof(PreUpdateRumble));
                }

                private static bool PreUpdateDoubleAxisCommand(
                    InputBinding ____gamepadHorzBinding,
                    InputBinding ____gamepadVertBinding,
                    ref Vector2 ____value,
                    ref Vector2 ____cameraLookValue
                )
                {
                    var axisX = InputTranslator.GetAxisName(____gamepadHorzBinding.axisID);
                    var axisY = InputTranslator.GetAxisName(____gamepadVertBinding.axisID);
                    if (_singleAxes.ContainsKey(axisX))
                    {
                        ____value.x = _singleAxes[axisX];
                        ____cameraLookValue.x = _singleAxes[axisX];
                    }
                    if (_singleAxes.ContainsKey(axisY))
                    {
                        ____value.y = _singleAxes[axisY];
                        ____cameraLookValue.y = _singleAxes[axisY];
                    }
                    if (____value.sqrMagnitude > 1f)
                    {
                        ____value.Normalize();
                    }
                    return false;
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unusued parameter is needed for return value passthrough.")]
                private static bool PostIsGamepadEnabled(bool __result)
                {
                    return true;
                }

                private static void ResetPadManagerKeyboard(ref bool ____gotKeyboardInputThisFrame)
                {
                    ____gotKeyboardInputThisFrame = false;
                }

                private static bool PreUpdateRumble(object[] ___m_theList, bool ___m_isEnabled)
                {
                    if (OWTime.IsPaused())
                    {
                        return false;
                    }

                    var a = Vector2.zero;
                    if (___m_isEnabled && OWInput.UsingGamepad())
                    {
                        var deltaTime = Time.deltaTime;
                        for (var i = 0; i < ___m_theList.Length; i++)
                        {
                            var rumble = ___m_theList[i];
                            var isAlive = (bool)rumble.GetType().GetMethod("IsAlive").Invoke(rumble, new object[] { });

                            if (isAlive)
                            {
                                rumble.Invoke("Update", deltaTime);
                            }

                            var isAliveAgain = (bool)rumble.GetType().GetMethod("IsAlive").Invoke(rumble, new object[] { });

                            if (isAliveAgain)
                            {

                                var power = (Vector2)rumble.GetType().GetMethod("GetPower").Invoke(rumble, new object[] { });
                                a += power;
                            }
                        }
                        a.x *= 1.42857146f;
                        a.y *= 1.42857146f;

                        var haptic = SteamVR_Actions.default_Haptic;
                        var frequency = 0.1f;
                        var amplitudeY = a.y * NomaiVR.Config.vibrationStrength;
                        var amplitudeX = a.x * NomaiVR.Config.vibrationStrength;
                        haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.RightHand);
                        haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.RightHand);
                        haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.LeftHand);
                        haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.LeftHand);
                    }

                    return false;
                }

                private static void PlayerResourcesAwake()
                {
                    _playerResources = GameObject.FindObjectOfType<PlayerResources>();
                }

                private static float GetAxis(float __result, string axisName)
                {
                    if (_singleAxes.ContainsKey(axisName))
                    {
                        return _singleAxes[axisName];
                    }
                    return __result;
                }

                private static bool SingleAxisUpdate(
                    SingleAxisCommand __instance,
                    InputBinding ____gamepadBinding,
                    ref float ____value,
                    ref bool ____newlyPressedThisFrame,
                    ref float ____lastValue,
                    ref float ____lastPressedDuration,
                    ref float ____pressedDuration,
                    ref float ____realtimeSinceLastUpdate
                )
                {
                    var positive = ____gamepadBinding.gamepadButtonPos;
                    var negative = ____gamepadBinding.gamepadButtonNeg;

                    ____newlyPressedThisFrame = false;
                    ____lastValue = ____value;
                    ____value = 0f;


                    if (_buttons.ContainsKey(positive))
                    {
                        ____value += _buttons[positive];
                    }

                    if (_buttons.ContainsKey(negative))
                    {
                        ____value -= _buttons[negative];
                    }

                    var axis = InputTranslator.GetAxisName(____gamepadBinding.axisID);
                    if (_singleAxes.ContainsKey(axis))
                    {
                        ____value += _singleAxes[axis] * ____gamepadBinding.axisDirection;
                    }

                    ____lastPressedDuration = ____pressedDuration;
                    ____pressedDuration = ((!__instance.IsPressed()) ? 0f : (____pressedDuration + (Time.realtimeSinceStartup - ____realtimeSinceLastUpdate)));
                    ____realtimeSinceLastUpdate = Time.realtimeSinceStartup;

                    return false;
                }

                private static bool OWInputUpdate(ref bool ____usingGamepad)
                {
                    ____usingGamepad = true;
                    return false;
                }

                private static void PostEnableListenForAllJoysticks()
                {
                    ReplaceInputs();
                }
            }
        }
    }
}
