using OWML.ModHelper.Events;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class ControllerInput : NomaiVRModule<ControllerInput.Behaviour, ControllerInput.Behaviour.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        public class Behaviour : MonoBehaviour
        {
            private static Behaviour _instance;
            private static Dictionary<JoystickButton, float> _buttons;
            private static Dictionary<string, float> _singleAxes;
            private static Dictionary<DoubleAxis, Vector2> _doubleAxes;
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
                _doubleAxes = new Dictionary<DoubleAxis, Vector2>();

                SteamVR_Actions.default_Jump.onChange += CreateButtonHandler(JoystickButton.FaceDown);
                SteamVR_Actions.default_Back.onChange += OnBackChange;
                SteamVR_Actions.default_Interact.onChange += OnPrimaryActionChange;
                SteamVR_Actions.default_RoolMode.onChange += CreateButtonHandler(JoystickButton.LeftBumper);
                SteamVR_Actions.default_Grip.onChange += OnGripChange;

                SteamVR_Actions.default_Menu.onChange += CreateButtonHandler(JoystickButton.Start);
                SteamVR_Actions.default_Map.onChange += CreateButtonHandler(JoystickButton.Select);

                SteamVR_Actions.default_ThrustDown.onChange += CreateSingleAxisHandler(XboxAxis.leftTrigger);
                SteamVR_Actions.default_ThrustUp.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);
                SteamVR_Actions.default_ThrustUp.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);

                SteamVR_Actions.default_Move.onChange += CreateDoubleAxisHandler(XboxAxis.leftStick, XboxAxis.leftStickX, XboxAxis.leftStickY);
                SteamVR_Actions.default_Look.onChange += CreateDoubleAxisHandler(XboxAxis.rightStick, XboxAxis.rightStickX, XboxAxis.rightStickY);

                GlobalMessenger.AddListener("WakeUp", OnWakeUp);

                // TODO Look into this missing method.
                // OWInput.EnableListenForAllJoysticks(true);
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

            public static void SimulateInput(SingleAxis axis, float value)
            {
                _singleAxes[axis.GetInputAxisName(0)] = value;
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(SingleAxis singleAxis, int axisDirection)
            {
                return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) =>
                {
                    _singleAxes[singleAxis.GetInputAxisName(0)] = axisDirection * Mathf.Round(newAxis * 10) / 10;
                };
            }

            private static SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(SingleAxis singleAxis)
            {
                return CreateSingleAxisHandler(singleAxis, 1);
            }

            private static SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler(JoystickButton button)
            {
                return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) =>
                {
                    _buttons[button] = newState ? 1 : 0;
                };
            }

            private static SteamVR_Action_Vector2.ChangeHandler CreateDoubleAxisHandler(DoubleAxis doubleAxis, SingleAxis singleX, SingleAxis singleY)
            {
                return (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) =>
                {
                    var x = Mathf.Round(axis.x * 100) / 100;
                    var y = Mathf.Round(axis.y * 100) / 100;
                    _doubleAxes[doubleAxis] = new Vector2(x, y);
                    _singleAxes[singleX.GetInputAxisName(0)] = x;
                    _singleAxes[singleY.GetInputAxisName(0)] = y;
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

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Pre<SingleAxisCommand>("Update", typeof(Patch), nameof(SingleAxisUpdate));
                    NomaiVR.Pre<OWInput>("UpdateActiveInputDevice", typeof(Patch), nameof(OWInputUpdate));
                    NomaiVR.Pre<OWInput>("EnableListenForAllJoysticks", typeof(Patch), nameof(PostEnableListanForAllJoysticks));
                    NomaiVR.Pre<OWInput>("Awake", typeof(Patch), nameof(PostEnableListanForAllJoysticks));
                    NomaiVR.Post<PadEZ.PadManager>("GetAxis", typeof(Patch), nameof(GetAxis));
                    NomaiVR.Post<PlayerResources>("Awake", typeof(Patch), nameof(PlayerResourcesAwake));

                    var rumbleMethod = typeof(RumbleManager).GetAnyMethod("Update");
                    NomaiVR.Helper.HarmonyHelper.AddPrefix(rumbleMethod, typeof(Patch), nameof(PreUpdateRumble));
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
                    JoystickButton ____JoystickButtonPositive,
                    JoystickButton ____JoystickButtonNegative,
                    ref float ____value,
                    ref bool ____newlyPressedThisFrame,
                    ref float ____lastValue,
                    ref float ____lastPressedDuration,
                    ref float ____pressedDuration,
                    ref float ____realtimeSinceLastUpdate
                )
                {
                    if (____JoystickButtonPositive == JoystickButton.None && ____JoystickButtonNegative == JoystickButton.None)
                    {
                        return true;
                    }

                    ____newlyPressedThisFrame = false;
                    ____lastValue = ____value;
                    ____value = 0f;


                    if (_buttons.ContainsKey(____JoystickButtonPositive))
                    {
                        ____value += _buttons[____JoystickButtonPositive];
                    }

                    if (_buttons.ContainsKey(____JoystickButtonNegative))
                    {
                        ____value -= _buttons[____JoystickButtonNegative];
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

                private static void SetCommandButton(SingleAxisCommand command, JoystickButton button)
                {
                    command.SetValue("_gamepadBinding", new InputBinding(button));
                }

                private static void PostEnableListanForAllJoysticks()
                {
                    SetCommandButton(InputLibrary.landingCamera, JoystickButton.DPadDown);
                    SetCommandButton(InputLibrary.signalscope, JoystickButton.DPadRight);
                    SetCommandButton(InputLibrary.tabL, JoystickButton.None);
                    SetCommandButton(InputLibrary.tabR, JoystickButton.None);
                    SetCommandButton(InputLibrary.setDefaults, JoystickButton.None);
                    SetCommandButton(InputLibrary.confirm, JoystickButton.None);
                    SetCommandButton(InputLibrary.confirm2, JoystickButton.None);
                }
            }
        }
    }
}
