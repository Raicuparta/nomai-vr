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
        private static VRActionInput[] baseActions;
        private static bool _isActionInputsInitialized;
        public static bool IsInputEnabled = true;

        public class Behaviour : MonoBehaviour
        {
            public static event Action BindingsChanged;
            public static bool IsGripping { get; private set; }
            public static bool MovementOnLeftHand => _movementAction.activeDevice == SteamVR_Input_Sources.LeftHand;
            private const float deadZone = 0.25f;
            private const float holdDuration = 0.3f;

            private static Behaviour _instance;
            private static OverridableSteamVRAction _movementAction;
            private static Dictionary<int, float> _buttons;
            private static Dictionary<int, float> _axes;
            private static PlayerResources _playerResources;

            private bool _isLeftDominant;
            private bool _isUsingTools;
            private System.Collections.IEnumerator _executeBaseBindingsChanged;
            private System.Collections.IEnumerator _executeBaseBindingsOverriden;
            private SteamVR_Input_Sources? _lastToolInputSource;

            internal void Awake()
            {
                OpenVR.Input.SetActionManifestPath(NomaiVR.Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
            }

            internal void Start()
            {
                _instance = this;

                //We need to use ints here cause the Enums would use Mono's equality comparer which allocates some bytes for each call
                _buttons = new Dictionary<int, float>();
                _axes = new Dictionary<int, float>();

                SetUpSteamVRActionHandlers();
                ReplaceInputs();
                SetUpActionInputs();
                UpdateHandDominance();

                ModSettings.OnConfigChange += OnSettingsChanged;
            }

            internal void OnSettingsChanged()
            {
                if(_isLeftDominant != ModSettings.LeftHandDominant)
                {
                    _isLeftDominant = ModSettings.LeftHandDominant;
                    UpdateHandDominance();
                }
            }

            internal void UpdateHandDominance()
            {
                if (ModSettings.LeftHandDominant)
                    SteamVR_Actions.inverted.Activate(priority: 1);
                else
                    SteamVR_Actions.inverted.Deactivate();
            }

            internal void OnEnable()
            {
                SteamVR_Events.System(EVREventType.VREvent_InputFocusChanged).Listen(OnInputFocus);
            }

            internal void OnDisable()
            {
                SteamVR_Events.System(EVREventType.VREvent_InputFocusChanged).Remove(OnInputFocus);
                StopAllCoroutines();
                _executeBaseBindingsOverriden = null;
                _executeBaseBindingsChanged = null;
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
                var invertedActionSet = SteamVR_Actions.inverted;
                var toolsActionSet = SteamVR_Actions.tools;
                var gripActionInput = new VRActionInput(defaultActionSet.Grip)
                {
                    HideHand = true
                };
                _movementAction = new OverridableSteamVRAction(defaultActionSet.Move, invertedActionSet.Move);

                buttonActions = new Dictionary<JoystickButton, VRActionInput>
                {
                    [JoystickButton.FaceDown] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Jump, invertedActionSet.Jump), TextHelper.GREEN),
                    [JoystickButton.FaceRight] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Back, invertedActionSet.Back), TextHelper.RED),
                    [JoystickButton.FaceLeft] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Interact, invertedActionSet.Interact), TextHelper.BLUE),
                    //TODO: For Now we lie about needing to press grip button in hand-held mode, needs to be removed after cockpit changes
                    [JoystickButton.RightBumper] = new VRActionInput(toolsActionSet.Use, TextHelper.BLUE, false, gripActionInput, isDynamic: true),
                    [JoystickButton.LeftStickClick] = new VRActionInput(toolsActionSet.Use, TextHelper.BLUE, true, gripActionInput, isDynamic: true),
                    [JoystickButton.FaceUp] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Interact, invertedActionSet.Interact), TextHelper.BLUE, true),
                    [JoystickButton.LeftBumper] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.RollMode, invertedActionSet.RollMode)),
                    [JoystickButton.Start] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Menu, invertedActionSet.Menu)),
                    [JoystickButton.Select] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Map, invertedActionSet.Map)),
                    [JoystickButton.LeftTrigger] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.ThrustDown, invertedActionSet.ThrustDown)),
                    [JoystickButton.RightTrigger] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.ThrustUp, invertedActionSet.ThrustUp))
                };

                axisActions = new Dictionary<AxisIdentifier, VRActionInput>
                {
                    [AxisIdentifier.CTRLR_LTRIGGER] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.ThrustDown, invertedActionSet.ThrustDown)),
                    [AxisIdentifier.CTRLR_RTRIGGER] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.ThrustUp, invertedActionSet.ThrustUp)),
                    [AxisIdentifier.CTRLR_LSTICK] = new VRActionInput(_movementAction),
                    [AxisIdentifier.CTRLR_LSTICKX] = new VRActionInput(_movementAction),
                    [AxisIdentifier.CTRLR_LSTICKY] = new VRActionInput(_movementAction),
                    [AxisIdentifier.CTRLR_RSTICK] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Look, invertedActionSet.Look)),
                    [AxisIdentifier.CTRLR_RSTICKX] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Look, invertedActionSet.Look)),
                    [AxisIdentifier.CTRLR_RSTICKY] = new VRActionInput(new OverridableSteamVRAction(defaultActionSet.Look, invertedActionSet.Look)),
                    [AxisIdentifier.CTRLR_DPADX] = new VRActionInput(toolsActionSet.DPad, isDynamic: true),
                    [AxisIdentifier.CTRLR_DPADY] = new VRActionInput(toolsActionSet.DPad, isDynamic: true)
                };

                otherActions = new VRActionInput[] { gripActionInput };
                toolsActions = GetActionsForSet(toolsActionSet).ToArray();
                baseActions = GetActionsForSet(defaultActionSet).Union(GetActionsForSet(invertedActionSet)).ToArray();
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

                foreach (var button in buttonActions.Values.Union(axisActions.Values).Union(otherActions))
                {
                    //These are a bit expensive
                    button.SetClickable(button.HasAxisWithSameName());
                    button.HideHand = !button.Dynamic && !button.HasOppositeHandButtonWithSameName();
                }

                _isActionInputsInitialized = true;
                BindingsChanged?.Invoke();

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

                SteamVR_Actions.inverted_Jump.onChange += CreateButtonHandler(JoystickButton.FaceDown);
                SteamVR_Actions.inverted_Back.onChange += OnBackChange;
                SteamVR_Actions.inverted_Interact.onChange += OnInteractChange;
                SteamVR_Actions.inverted_RoolMode.onChange += CreateButtonHandler(JoystickButton.LeftBumper);
                SteamVR_Actions.inverted_Grip.onChange += OnGripChange;
                SteamVR_Actions.inverted_Menu.onChange += CreateButtonHandler(JoystickButton.Start);
                SteamVR_Actions.inverted_Map.onChange += CreateButtonHandler(JoystickButton.Select);
                SteamVR_Actions.inverted_ThrustDown.onChange += CreateSingleAxisHandler(AxisIdentifier.CTRLR_LTRIGGER);
                SteamVR_Actions.inverted_ThrustUp.onChange += CreateSingleAxisHandler(AxisIdentifier.CTRLR_RTRIGGER);
                SteamVR_Actions.inverted_ThrustDown.onChange += CreateSingleAxisHandler(JoystickButton.LeftTrigger);
                SteamVR_Actions.inverted_ThrustUp.onChange += CreateSingleAxisHandler(JoystickButton.RightTrigger);
                SteamVR_Actions.inverted_Move.onChange += CreateDoubleAxisHandler(AxisIdentifier.CTRLR_LSTICKX, AxisIdentifier.CTRLR_LSTICKY);
                SteamVR_Actions.inverted_Look.onChange += CreateDoubleAxisHandler(AxisIdentifier.CTRLR_RSTICKX, AxisIdentifier.CTRLR_RSTICKY);

                SteamVR_Actions.tools_Use.AddOnChangeListener(OnToolUseChange, SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools_Use.AddOnChangeListener(OnToolUseChange, SteamVR_Input_Sources.RightHand);
                SteamVR_Actions.tools_DPad.AddOnChangeListener(CreateDoubleAxisHandler(AxisIdentifier.CTRLR_DPADX, AxisIdentifier.CTRLR_DPADY), SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools_DPad.AddOnChangeListener(CreateDoubleAxisHandler(AxisIdentifier.CTRLR_DPADX, AxisIdentifier.CTRLR_DPADY), SteamVR_Input_Sources.RightHand);

                //Update Prompts events
                foreach (var action in SteamVR_Actions._default.nonVisualInActions)
                    RegisterToActiveBindingChanged(action, OnBaseBindingChanged);
                foreach (var action in SteamVR_Actions.inverted.nonVisualInActions)
                    RegisterToActiveBindingChanged(action, OnBaseBindingChanged);

                //Add Events used to update tool prompts
                SteamVR_Actions.tools_Use.AddOnActiveBindingChangeListener(ToolModeBound, SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools_Use.AddOnActiveBindingChangeListener(ToolModeBound, SteamVR_Input_Sources.RightHand);
            }

            private void RegisterToActiveBindingChanged(ISteamVR_Action_In actionIn, Action<ISteamVR_Action, SteamVR_Input_Sources, bool> changeHandler)
            {
                if (actionIn is SteamVR_Action_Boolean boolAction)
                    boolAction.onActiveBindingChange += (action, source, active) => changeHandler(action, source, active);
                else if (actionIn is SteamVR_Action_Single singleAction)
                    singleAction.onActiveBindingChange += (action, source, active) => changeHandler(action, source, active);
                else if (actionIn is SteamVR_Action_Vector2 vectorAction)
                    vectorAction.onActiveBindingChange += (action, source, active) => changeHandler(action, source, active);
            }


            private void OnBaseBindingChanged(ISteamVR_Action fromAction, SteamVR_Input_Sources fromSource, bool active)
            {
                if(active && _executeBaseBindingsChanged == null && fromAction != null && fromAction.active)
                    StartCoroutine(_executeBaseBindingsChanged = ProcessBaseBindingsChanged());

                //If an action is unbound, set the related axis to 0
                if (!active && _executeBaseBindingsOverriden == null && fromAction != null)
                    StartCoroutine(_executeBaseBindingsOverriden = ResetAllUnboundAxes());
            }

            private void OnBackChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                //Only Allow Pressing Back while not in tool mode
                if (!_isUsingTools)
                {
                    _buttons[(int)JoystickButton.FaceRight] = newState ? 1 : 0;
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

                //Don't Interact when holding a tool
                if (ToolHelper.IsUsingAnyTool(ToolGroup.None) || ToolHelper.IsUsingAnyTool(ToolGroup.Suit))
                    return;

                if (!SceneHelper.IsInGame())
                {
                    _buttons[(int)JoystickButton.FaceLeft] = value;
                    return;
                }

                var button = JoystickButton.FaceLeft;
                var repairPrompt = LaserPointer.Behaviour.Instance?.Manipulator?._repairScreenPrompt;
                var isRepairPromptVisible = repairPrompt != null && repairPrompt.IsVisible();
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
                    _buttons[(int)button] = value;
                }
            }

            private IEnumerator<WaitForSecondsRealtime> _delayedToolUse = null;
            private void OnToolUseChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
            {
                var value = newState ? 1 : 0;

                if (!SceneHelper.IsInGame())
                {
                    _buttons[(int)JoystickButton.FaceLeft] = value;
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
                    _buttons[(int)button] = value;
                }
            }

            private IEnumerator<WaitForEndOfFrame> ResetAllUnboundAxes()
            {
                yield return new WaitForEndOfFrame();
                foreach (var axis in axisActions.Keys)
                    if (!axisActions[axis].Active)
                        SimulateInput(axis, 0.0f);
                _executeBaseBindingsOverriden = null;
            }

            private IEnumerator<WaitForEndOfFrame> ProcessBaseBindingsChanged()
            {
                yield return new WaitForEndOfFrame();
                _isActionInputsInitialized = false;
                InitializeActionInputs();
                InputPrompts.Behaviour.UpdatePrompts(baseActions);

                //Reset all Axes (since the trigger used to change bindings is probably still pressed)
                foreach (var axis in _axes.Keys.ToArray())
                    _axes[axis] = 0.0f;

                _executeBaseBindingsChanged = null;
            }

            private static IEnumerator<WaitForSecondsRealtime> ResetInput(JoystickButton button)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                SimulateInput(button, 0);
            }

            public static void SimulateInput(JoystickButton button)
            {
                _buttons[(int)button] = 1;
                _instance.StartCoroutine(ResetInput(button));
            }

            public static void SimulateInput(JoystickButton button, float value)
            {
                _buttons[(int)button] = value;
            }

            public static void SimulateInput(AxisIdentifier axis, float value)
            {
                _axes[(int)axis] = value;
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(AxisIdentifier axis, int axisDirection = 1)
            {
                return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) =>
                {
                    _axes[(int)axis] = axisDirection * Mathf.Round(newAxis * 10) / 10;
                };
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(JoystickButton button)
            {
                return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) =>
                {
                    _buttons[(int)button] = newAxis;
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
                    _buttons[(int)button] = newState ? 1 : 0;
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

                    var x = Mathf.Round(axis.x * 100) / 100;
                    var y = Mathf.Round(axis.y * 100) / 100;
                    _axes[(int)axisX] = x;
                    _axes[(int)axisY] = y;
                };
            }

            private void EnterToolMode(SteamVR_Input_Sources inputSource)
            {
                //Enables the tools override for the proper hand and change affected prompts
                SteamVR_Actions.tools.Activate(priority: 2, activateForSource: inputSource);
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
                var dominantHandSource = ModSettings.LeftHandDominant ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
                var nonDominantHand = !ModSettings.LeftHandDominant ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;

                //De-Activates the tools action-set (stops overriting same buttons)
                SteamVR_Actions.tools.Deactivate(nonDominantHand);

                //Restores mainhand prompts, a bit of a hack...
                SteamVR_Actions.tools.Activate(dominantHandSource, priority: 2);
                SteamVR_Actions.tools.Deactivate(dominantHandSource);
            }

            internal void Update()
            {
                //Ship Tools will have their buttons in cockpit
                //FIXME: Remove IsGripping, use physical buttons in cockpit to avoid confusion
                bool inMenus = InputHelper.IsUIInteractionMode(true);
                bool isUsingPlayerTools = ToolHelper.IsUsingAnyTool() && (!ToolHelper.IsUsingAnyTool(ToolGroup.Ship) || IsGripping);
                bool isUsingStationaryTools = InputHelper.IsStationaryToolMode() && IsGripping;
                bool canUseTools = !inMenus && (isUsingPlayerTools || isUsingStationaryTools);
                if (!_isUsingTools && canUseTools)
                {
                    _isUsingTools = true;

                    var interactingHandSource = VRToolSwapper.InteractingHand?.InputSource;
                    var dominantHandSource = ModSettings.LeftHandDominant ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
                    EnterToolMode(interactingHandSource ?? dominantHandSource);
                } 
                else if(_isUsingTools && !canUseTools)
                {
                    _isUsingTools = false;
                    ExitToolMode();
                }
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

                    float x = 0;
                    float y = 0;
                    if (_axes.ContainsKey((int)____gamepadHorzBinding.axisID))
                    {
                        x = DeadzonedValue(_axes[(int)____gamepadHorzBinding.axisID]);
                    }
                    if (_axes.ContainsKey((int)____gamepadVertBinding.axisID))
                    {
                        y = DeadzonedValue(_axes[(int)____gamepadVertBinding.axisID]);
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


                    if (_buttons.ContainsKey((int)positive))
                    {
                        ____value += _buttons[(int)positive];
                    }

                    if (_buttons.ContainsKey((int)negative))
                    {
                        ____value -= _buttons[(int)negative];
                    }

                    if (_axes.ContainsKey((int)____gamepadBinding.axisID))
                    {
                        ____value += DeadzonedValue(_axes[(int)____gamepadBinding.axisID] * ____gamepadBinding.axisDirection);
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
