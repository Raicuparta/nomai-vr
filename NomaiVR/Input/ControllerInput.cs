using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class ControllerInput : NomaiVRModule<ControllerInput.Behaviour, ControllerInput.Behaviour.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;
        public static Dictionary<JoystickButton, VRActionInput> buttonActions;
        public static Dictionary<AxisIdentifier, VRActionInput> axisActions;
        public static VRActionInput[] otherActions;
        private static VRActionInput[] toolsActions;
        private static bool _isActionInputsInitialized;
        public static bool IsInputEnabled = true;

        public class Behaviour : MonoBehaviour
        {
            public static bool IsGripping { get; private set; }

            private const float deadZone = 0.25f;
            private const float holdDuration = 0.3f;

            private static Behaviour _instance;
            private static Dictionary<JoystickButton, float> _buttons;
            private static Dictionary<string, float> _axes;
            private static PlayerResources _playerResources;

            private bool _isUsingTools;
            private ScreenPrompt _repairPrompt;
            private SteamVR_Input_Sources? _lastToolInputSource;

            internal void Awake()
            {
                OpenVR.Input.SetActionManifestPath(NomaiVR.Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
            }

            internal void Start()
            {
                _instance = this;
                _buttons = new Dictionary<JoystickButton, float>();
                _axes = new Dictionary<string, float>();

                SetUpSteamVRActionHandlers();
                ReplaceInputs();
                SetUpActionInputs();
                GlobalMessenger.AddListener("WakeUp", OnWakeUp);

                //Wait for binding state change
                SteamVR_Actions._default.LeftHand.onActiveBindingChange += (pose, source, val) => { if (val) InitializeActionInputs(); };
            }

            internal void OnEnable()
            {
                SteamVR_Events.System(EVREventType.VREvent_InputFocusChanged).Listen(OnInputFocus);
            }

            internal void OnDisable()
            {
                SteamVR_Events.System(EVREventType.VREvent_InputFocusChanged).Remove(OnInputFocus);
                StopAllCoroutines();
            }

            private void OnInputFocus(VREvent_t arg)
            {
                if (!OWTime.IsPaused())
                {
                    SimulateInput(JoystickButton.Start);
                }
            }

            private static void SetUpActionInputs()
            {
                var defaultActionSet = SteamVR_Actions._default;
                defaultActionSet.Activate(disableAllOtherActionSets: true);
                var toolsActionSet = SteamVR_Actions.tools;
                var gripActionInput = new VRActionInput(defaultActionSet.Grip);
                gripActionInput.HideHand = true;

                buttonActions = new Dictionary<JoystickButton, VRActionInput>
                {
                    [JoystickButton.FaceDown] = new VRActionInput(defaultActionSet.Jump, TextHelper.GREEN),
                    [JoystickButton.FaceRight] = new VRActionInput(defaultActionSet.Back, TextHelper.RED),
                    [JoystickButton.FaceLeft] = new VRActionInput(defaultActionSet.Interact, TextHelper.BLUE),
                    //TODO: For Now we lie about needing to press grip button in hand-held mode, needs to be removed after cockpit changes
                    [JoystickButton.RightBumper] = new VRActionInput(toolsActionSet.Use, TextHelper.BLUE, false, gripActionInput, isDynamic: true),
                    [JoystickButton.LeftStickClick] = new VRActionInput(toolsActionSet.Use, TextHelper.BLUE, true, gripActionInput, isDynamic: true),
                    [JoystickButton.FaceUp] = new VRActionInput(defaultActionSet.Interact, TextHelper.BLUE, true),
                    [JoystickButton.LeftBumper] = new VRActionInput(defaultActionSet.RollMode),
                    [JoystickButton.Start] = new VRActionInput(defaultActionSet.Menu),
                    [JoystickButton.Select] = new VRActionInput(defaultActionSet.Map),
                    [JoystickButton.LeftTrigger] = new VRActionInput(defaultActionSet.ThrustDown),
                    [JoystickButton.RightTrigger] = new VRActionInput(defaultActionSet.ThrustUp)
                };

                axisActions = new Dictionary<AxisIdentifier, VRActionInput>
                {
                    [AxisIdentifier.CTRLR_LTRIGGER] = new VRActionInput(defaultActionSet.ThrustDown),
                    [AxisIdentifier.CTRLR_RTRIGGER] = new VRActionInput(defaultActionSet.ThrustUp),
                    [AxisIdentifier.CTRLR_LSTICK] = new VRActionInput(defaultActionSet.Move),
                    [AxisIdentifier.CTRLR_LSTICKX] = new VRActionInput(defaultActionSet.Move),
                    [AxisIdentifier.CTRLR_LSTICKY] = new VRActionInput(defaultActionSet.Move),
                    [AxisIdentifier.CTRLR_RSTICK] = new VRActionInput(defaultActionSet.Look),
                    [AxisIdentifier.CTRLR_RSTICKX] = new VRActionInput(defaultActionSet.Look),
                    [AxisIdentifier.CTRLR_RSTICKY] = new VRActionInput(defaultActionSet.Look),
                    [AxisIdentifier.CTRLR_DPADX] = new VRActionInput(toolsActionSet.DPad, isDynamic: true),
                    [AxisIdentifier.CTRLR_DPADY] = new VRActionInput(toolsActionSet.DPad, isDynamic: true)
                };

                otherActions = new VRActionInput[] { gripActionInput };
                toolsActions = GetActionsForSet(toolsActionSet).ToArray();
            }

            public static IEnumerable<VRActionInput> GetActionsForSet(SteamVR_ActionSet actionSet)
            {
                return buttonActions.Values.Union(axisActions.Values).Union(otherActions)
                    .Where(x => x.DependsOnActionSet(actionSet));
            }

            public static void InitializeActionInputs()
            {
                if (_isActionInputsInitialized)
                {
                    return;
                }

                foreach (var axisEntry in axisActions)
                {
                    axisEntry.Value.Initialize();
                }
                foreach (var buttonEntry in buttonActions)
                {
                    buttonEntry.Value.Initialize();
                }
                foreach (var otherAction in otherActions)
                {
                    otherAction.Initialize();
                }

                foreach (var buttonEntry in buttonActions)
                {
                    var button = buttonEntry.Value;
                    if (button.HasAxisWithSameName())
                    {
                        button.SetAsClickable();
                    }
                    if (!button.Dynamic && !button.HasOppositeHandButtonWithSameName())
                    {
                        button.HideHand = true;
                    }
                }

                _isActionInputsInitialized = true;

                // Only need to pause the game until prompts are set up.
                // After that, forcing pauses can break stuff, so better disable it here.
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = false;
            }

            private void SetUpSteamVRActionHandlers()
            {
                SteamVR_Actions.default_Jump.onChange += CreateButtonHandler(JoystickButton.FaceDown);
                SteamVR_Actions.default_Back.onChange += OnBackChange;
                SteamVR_Actions.default_Interact.onChange += OnInteractChange;
                SteamVR_Actions.default_RoolMode.onChange += CreateButtonHandler(JoystickButton.LeftBumper);
                SteamVR_Actions.default_Grip.onChange += OnGripChange;
                SteamVR_Actions.default_Menu.onChange += CreateButtonHandler(JoystickButton.Start);
                SteamVR_Actions.default_Map.onChange += CreateButtonHandler(JoystickButton.Select);
                SteamVR_Actions.default_ThrustDown.onChange += CreateSingleAxisHandler(AxisIdentifier.CTRLR_LTRIGGER);
                SteamVR_Actions.default_ThrustUp.onChange += CreateSingleAxisHandler(AxisIdentifier.CTRLR_RTRIGGER);
                SteamVR_Actions.default_ThrustDown.onChange += CreateSingleAxisHandler(JoystickButton.LeftTrigger);
                SteamVR_Actions.default_ThrustUp.onChange += CreateSingleAxisHandler(JoystickButton.RightTrigger);
                SteamVR_Actions.default_Move.onChange += CreateDoubleAxisHandler(AxisIdentifier.CTRLR_LSTICKX, AxisIdentifier.CTRLR_LSTICKY);
                SteamVR_Actions.default_Look.onChange += CreateDoubleAxisHandler(AxisIdentifier.CTRLR_RSTICKX, AxisIdentifier.CTRLR_RSTICKY);

                SteamVR_Actions.tools_Use.AddOnChangeListener(OnToolUseChange, SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools_Use.AddOnChangeListener(OnToolUseChange, SteamVR_Input_Sources.RightHand);
                SteamVR_Actions.tools_DPad.AddOnChangeListener(CreateDoubleAxisHandler(AxisIdentifier.CTRLR_DPADX, AxisIdentifier.CTRLR_DPADY), SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools_DPad.AddOnChangeListener(CreateDoubleAxisHandler(AxisIdentifier.CTRLR_DPADX, AxisIdentifier.CTRLR_DPADY), SteamVR_Input_Sources.RightHand);

                //Add Events used to update tool prompts
                SteamVR_Actions.tools_Use.AddOnActiveBindingChangeListener(ToolModeBound, SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools_Use.AddOnActiveBindingChangeListener(ToolModeBound, SteamVR_Input_Sources.RightHand);
            }

            private void OnWakeUp()
            {
                _repairPrompt = FindObjectOfType<FirstPersonManipulator>()._repairScreenPrompt;
            }

            private void OnBackChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                //Only Allow Pressing Back while not in tool mode
                if (!_isUsingTools)
                {
                    _buttons[JoystickButton.FaceRight] = newState ? 1 : 0;
                }
            }

            private void OnGripChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                IsGripping = newState;
            }

            private IEnumerator<WaitForSecondsRealtime> _delayedInteract = null;
            private void OnInteractChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                var value = newState ? 1 : 0;

                if (!SceneHelper.IsInGame())
                {
                    _buttons[JoystickButton.FaceLeft] = value;
                    return;
                }

                var button = JoystickButton.FaceLeft;

                var isRepairPromptVisible = _repairPrompt != null && _repairPrompt.IsVisible();
                var canRepairSuit = _playerResources.IsSuitPunctured() && OWInput.IsInputMode(InputMode.Character) && !ToolHelper.Swapper.IsSuitPatchingBlocked();

                if (!isRepairPromptVisible && !canRepairSuit)
                {
                    if (newState)
                    {
                        if (_delayedInteract != null)
                            StopCoroutine(_delayedInteract);

                        float waitTime = holdDuration - (Time.realtimeSinceStartup - fromAction.GetTimeLastChanged(fromSource));
                        _delayedInteract = DelayedPress(waitTime, JoystickButton.FaceUp, () => _delayedInteract = null);
                        StartCoroutine(_delayedInteract);
                    }
                    else
                    {
                        if (_delayedInteract != null)
                        {
                            StopCoroutine(_delayedInteract);
                            _delayedInteract = null;
                            SimulateInput(button);
                        }
                    }
                }
                else
                {
                    _buttons[button] = value;
                }
            }

            private IEnumerator<WaitForSecondsRealtime> _delayedToolUse = null;
            private void OnToolUseChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                //TODO: Maybe we can trim some things?
                var value = newState ? 1 : 0;

                if (!SceneHelper.IsInGame())
                {
                    _buttons[JoystickButton.FaceLeft] = value;
                    return;
                }

                var button = JoystickButton.RightBumper;
                if (!ToolHelper.Swapper.IsInToolMode(ToolMode.Translator))
                {
                    if (newState)
                    {
                        if (_delayedToolUse != null)
                            StopCoroutine(_delayedToolUse);

                        float waitTime = holdDuration - (Time.realtimeSinceStartup - fromAction.GetTimeLastChanged(fromSource));
                        _delayedToolUse = DelayedPress(waitTime, JoystickButton.LeftStickClick, () => _delayedToolUse = null);
                        StartCoroutine(_delayedToolUse);
                    }
                    else
                    {
                        if (_delayedToolUse != null)
                        {
                            StopCoroutine(_delayedToolUse);
                            _delayedToolUse = null;
                            SimulateInput(button);
                        }
                    }
                }
                else
                {
                    _buttons[button] = value;
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
                _axes[InputTranslator.GetAxisName(axis)] = value;
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(AxisIdentifier axis, int axisDirection = 1)
            {
                return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) =>
                {
                    var axisName = InputTranslator.GetAxisName(axis);
                    _axes[axisName] = axisDirection * Mathf.Round(newAxis * 10) / 10;
                };
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(JoystickButton button)
            {
                return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) =>
                {
                    _buttons[button] = newAxis;
                };
            }

            private static SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler(JoystickButton button, Func<bool> predicate = null)
            {
                return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) =>
                {
                    if (predicate != null && !predicate())
                    {
                        return;
                    }
                    _buttons[button] = newState ? 1 : 0;
                };
            }

            private static SteamVR_Action_Vector2.ChangeHandler CreateDoubleAxisHandler(AxisIdentifier axisX, AxisIdentifier axisY, Func<bool> predicate = null)
            {
                return (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) =>
                {
                    if (predicate != null && !predicate())
                    {
                        return;
                    }

                    var axisNameX = InputTranslator.GetAxisName(axisX);
                    var axisNameY = InputTranslator.GetAxisName(axisY);
                    var x = Mathf.Round(axis.x * 100) / 100;
                    var y = Mathf.Round(axis.y * 100) / 100;
                    _axes[axisNameX] = x;
                    _axes[axisNameY] = y;
                };
            }

            private void EnterToolMode(bool rightHand = true)
            {
                //Enables the tools override for the proper hand and change affected prompts
                SteamVR_Actions.tools.Activate(priority: 1, activateForSource: rightHand ? SteamVR_Input_Sources.RightHand : SteamVR_Input_Sources.LeftHand);
            }

            private void ToolModeBound(SteamVR_Action_Boolean action, SteamVR_Input_Sources inputSource, bool newValue)
            {
                if (newValue)
                {
                    if (_lastToolInputSource != inputSource)
                    {
                        foreach (var vrActionInput in toolsActions)
                        {
                            vrActionInput.BindSource(inputSource);
                            vrActionInput.Initialize();
                        }
                        InputPrompts.Behaviour.UpdatePrompts(toolsActions);
                        _lastToolInputSource = inputSource;
                    }
                }
            }

            private void ExitToolMode()
            {
                //De-Activates the tools action-set (stops overriting same buttons)
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.RightHand);
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.LeftHand);
            }

            internal void Update()
            {
                //Ship Tools will have their buttons in cockpit
                //Fixme: Remove IsGripping, use physical buttons in cockpit to avoid confusion
                bool canUseTools = !InputHelper.IsUIInteractionMode(true) && ToolHelper.IsUsingAnyTool() && (!ToolHelper.IsUsingAnyTool(ToolGroup.Ship) || IsGripping);
                if (!_isUsingTools && canUseTools)
                {
                    _isUsingTools = true;
                    EnterToolMode();
                } 
                else if(_isUsingTools && !canUseTools)
                {
                    _isUsingTools = false;
                    ExitToolMode();
                }
            }

            private static IEnumerator<WaitForEndOfFrame> DelayedInvoke(Action action, int frames)
            {
                for(int i = 0; i < frames; i++)
                    yield return null;
                yield return new WaitForEndOfFrame();

                action();
            }

            private static IEnumerator<WaitForSecondsRealtime> DelayedPress(float time, JoystickButton button, Action then = null)
            {
                yield return new WaitForSecondsRealtime(time);
                SimulateInput(button);
                then?.Invoke();
            }

            private static void SetGamepadBinding(SingleAxisCommand command, InputBinding binding)
            {
                command._gamepadBinding = binding;
            }

            private static void SetCommandButton(SingleAxisCommand command, JoystickButton button)
            {
                SetGamepadBinding(command, new InputBinding(button));
            }

            private static void SetCommandButton(SingleAxisCommand command, JoystickButton buttonPositive, JoystickButton buttonNegative)
            {
                SetGamepadBinding(command, new InputBinding(buttonPositive, buttonNegative));
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
                SetCommandButton(InputLibrary.mapZoom, JoystickButton.RightTrigger, JoystickButton.LeftTrigger);
                SetCommandButton(InputLibrary.scopeView, JoystickButton.RightBumper);
                SetCommandButton(InputLibrary.probeRetrieve, JoystickButton.LeftStickClick);
                SetCommandButton(InputLibrary.probeForward, JoystickButton.RightBumper);
                SetCommandButton(InputLibrary.translate, JoystickButton.RightBumper);
                SetCommandButton(InputLibrary.autopilot, JoystickButton.FaceUp);
                SetCommandButton(InputLibrary.lockOn, JoystickButton.FaceLeft);
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<SingleAxisCommand>("UpdateInputCommand", nameof(SingleAxisUpdate));
                    Prefix<OWInput>("UpdateActiveInputDevice", nameof(OWInputUpdate));
                    Prefix<OWInput>("Awake", nameof(PostEnableListenForAllJoysticks));
                    Postfix<PlayerResources>("Awake", nameof(PlayerResourcesAwake));
                    Postfix<PadEZ.PadManager_OW>("GetKey", nameof(ResetPadManagerKeyboard));
                    Postfix<PadEZ.PadManager_OW>("GetKeyDown", nameof(ResetPadManagerKeyboard));
                    Postfix<PadEZ.PadManager_OW>("GetKeyUp", nameof(ResetPadManagerKeyboard));
                    Postfix<OWInput>("IsGamepadEnabled", nameof(PostIsGamepadEnabled));
                    Postfix<PadEZ.PadManager_OW>("IsGamepadActive", nameof(PostIsGamepadEnabled));
                    Prefix<DoubleAxisCommand>("UpdateInputCommand", nameof(PreUpdateDoubleAxisCommand));
                    Prefix<SubmitActionMenu>("Submit", nameof(PreSubmitActionMenu));
                    Prefix(typeof(RumbleManager).GetAnyMethod("Update"), nameof(PreUpdateRumble));

                    //This method is only used in the intro screen and can break the intro sequence
                    //It is checking for keys the game and the mod doesn't use, the intro sequence is still skippable without it
                    Prefix<OWInput>("GetAnyJoystickButtonPressed", nameof(PrefixGetAnyJoystickButtonPressed));
                }

                private static bool PreSubmitActionMenu(SubmitActionMenu __instance)
                {
                    if (__instance.gameObject.name == "UIElement-RemapControls")
                    {
                        SteamVR_Input.OpenBindingUI(SteamVR_Actions._default);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                private static float DeadzonedValue(float axisValue)
                {
                    var absValue = Mathf.Abs(axisValue);
                    return Mathf.Sign(axisValue) * Mathf.InverseLerp(deadZone, 1f - deadZone, absValue);
                }

                private static bool PreUpdateDoubleAxisCommand(
                    InputBinding ____gamepadHorzBinding,
                    InputBinding ____gamepadVertBinding,
                    ref Vector2 ____value,
                    ref Vector2 ____cameraLookValue
                )
                {
                    if (!IsInputEnabled)
                    {
                        return false;
                    }

                    var axisX = InputTranslator.GetAxisName(____gamepadHorzBinding.axisID);
                    var axisY = InputTranslator.GetAxisName(____gamepadVertBinding.axisID);
                    float x = 0;
                    float y = 0;
                    if (_axes.ContainsKey(axisX))
                    {
                        x = DeadzonedValue(_axes[axisX]);
                    }
                    if (_axes.ContainsKey(axisY))
                    {
                        y = DeadzonedValue(_axes[axisY]);
                    }

                    ____value.x = x;
                    ____cameraLookValue.x = x;
                    ____value.y = y;
                    ____cameraLookValue.y = y;

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

                private static bool PrefixGetAnyJoystickButtonPressed(ref bool __result)
                {
                    __result = false;
                    return false;
                }

                private static void ResetPadManagerKeyboard(ref bool ____gotKeyboardInputThisFrame)
                {
                    ____gotKeyboardInputThisFrame = false;
                }

                private static bool PreUpdateRumble(RumbleManager.Rumble[] ___m_theList, bool ___m_isEnabled)
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

                            if (rumble.IsAlive())
                            {
                                rumble.Update(deltaTime);
                            }

                            if (rumble.IsAlive())
                            {
                                a += rumble.GetPower();
                            }
                        }
                        a.x *= 1.42857146f;
                        a.y *= 1.42857146f;

                        var haptic = SteamVR_Actions.default_Haptic;
                        var frequency = 0.1f;
                        var amplitudeY = a.y * ModSettings.VibrationStrength;
                        var amplitudeX = a.x * ModSettings.VibrationStrength;
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
                    if (!IsInputEnabled)
                    {
                        return false;
                    }

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
                    if (_axes.ContainsKey(axis))
                    {
                        ____value += DeadzonedValue(_axes[axis] * ____gamepadBinding.axisDirection);
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
