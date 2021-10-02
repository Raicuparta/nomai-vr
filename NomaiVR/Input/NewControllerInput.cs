using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.Input
{
    internal class NewControllerInput : NomaiVRModule<NomaiVRModule.EmptyBehaviour, NewControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private static readonly Dictionary<int, bool> simulatedBoolInputs = new Dictionary<int, bool>();

        public static void SimulateInputPress(InputCommandType commandType)
        {
            simulatedBoolInputs[(int)commandType] = true;
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.UpdateFromAction), nameof(PatchInputCommands));
                Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.UpdateFromAction), nameof(PatchInputCommands));
                Postfix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchInputCommands));
                Prefix<InputManager>(nameof(InputManager.Rumble), nameof(DoRumble));
                Postfix<InputManager>(nameof(InputManager.IsGamepadEnabled), nameof(ForceGamepadEnabled));
                Postfix<InputManager>(nameof(InputManager.UsingGamepad), nameof(ForceGamepadEnabled));

                VRToolSwapper.ToolEquipped += OnToolEquipped;
                VRToolSwapper.UnEquipped += OnToolUnequipped;
            }

            private void OnToolUnequipped()
            {
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.RightHand);
            }

            private void OnToolEquipped()
            {
                if (VRToolSwapper.InteractingHand != null)
                {
                    SteamVR_Actions.tools.Activate(VRToolSwapper.InteractingHand.isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand, 1);
                }
            }

            private static Vector2 AxisValue(bool value)
            {
                return new Vector2(value ? 1f : 0f, 0f);
            }

            private static Vector2 AxisValue(float value, bool clamp = false)
            {
                return new Vector2(clamp ? Mathf.Clamp(value, 0f, 1f) : value, 0f);
            }

            private static Vector2 AxisValue(ISteamVR_Action_Boolean action)
            {
                return AxisValue(action.state);
            }

            private static Vector2 AxisValue(ISteamVR_Action_Single action)
            {
                return new Vector2(action.axis, 0f);
            }

            private static void PatchInputCommands(AbstractCommands __instance)
            {
                var commandType = __instance.CommandType;
                var commandTypeKey = (int)commandType;
                if (simulatedBoolInputs.ContainsKey(commandTypeKey) && simulatedBoolInputs[commandTypeKey])
                {
                    __instance.AxisValue = AxisValue(true);
                    simulatedBoolInputs[(int)commandType] = false;
                    return;
                }

                var defaultActions = SteamVR_Actions._default;
                var toolActions = SteamVR_Actions.tools;
                switch (commandType)
                {
                    case InputCommandType.JUMP:
                    case InputCommandType.BOOST:
                        __instance.AxisValue = AxisValue(defaultActions.Jump);
                        break;
                    case InputCommandType.ENTER:
                    case InputCommandType.SELECT:
                        __instance.AxisValue = AxisValue(defaultActions.UISelect);
                        break;
                    case InputCommandType.UP:
                        __instance.AxisValue = AxisValue(defaultActions.Move.axis.y, true);
                        break;
                    case InputCommandType.DOWN:
                        __instance.AxisValue = AxisValue(-defaultActions.Move.axis.y, true);
                        break;
                    case InputCommandType.RIGHT:
                        __instance.AxisValue = AxisValue(defaultActions.Move.axis.x, true);
                        break;
                    case InputCommandType.LEFT:
                        __instance.AxisValue = AxisValue(-defaultActions.Move.axis.x, true);
                        break;
                    case InputCommandType.MAP:
                        __instance.AxisValue = AxisValue(defaultActions.Map);
                        break;
                    case InputCommandType.PAUSE:
                    case InputCommandType.CONFIRM:
                        __instance.AxisValue = AxisValue(defaultActions.Menu);
                        break;
                    case InputCommandType.ESCAPE:
                    case InputCommandType.CANCEL:
                        __instance.AxisValue = AxisValue(defaultActions.Back);
                        break;
                    case InputCommandType.INTERACT:
                    case InputCommandType.LOCKON:
                        __instance.AxisValue = AxisValue(!ToolsActive && defaultActions.Interact.state);
                        break;
                    case InputCommandType.LOOK:
                        __instance.AxisValue = defaultActions.Look.axis;
                        break;
                    case InputCommandType.LOOK_X:
                        __instance.AxisValue = defaultActions.Look.axis;
                        break;
                    case InputCommandType.LOOK_Y:
                        __instance.AxisValue = AxisValue(defaultActions.Look.axis.y);
                        break;
                    case InputCommandType.MOVE_XZ:
                        __instance.AxisValue = defaultActions.Move.axis;
                        break;
                    case InputCommandType.MOVE_X:
                        __instance.AxisValue = defaultActions.Move.axis;
                        break;
                    case InputCommandType.MOVE_Z:
                        __instance.AxisValue = AxisValue(defaultActions.Move.axis.y);
                        break;
                    case InputCommandType.THRUST_UP:
                        __instance.AxisValue = AxisValue(defaultActions.ThrustUp);
                        break;
                    case InputCommandType.THRUST_DOWN:
                        __instance.AxisValue = AxisValue(defaultActions.ThrustDown);
                        break;
                    case InputCommandType.ROLL_MODE:
                        __instance.AxisValue = AxisValue(defaultActions.RollMode);
                        break;
                    case InputCommandType.AUTOPILOT:
                        __instance.AxisValue = AxisValue(defaultActions.Autopilot);
                        break;
                    case InputCommandType.PROBELAUNCH:
                    case InputCommandType.PROBERETRIEVE:
                    case InputCommandType.SCOPEVIEW:
                    case InputCommandType.TOOL_PRIMARY:
                        __instance.AxisValue = ToolsActive ? AxisValue(toolActions.Use.GetState(SteamVR_Input_Sources.LeftHand) || toolActions.Use.GetState(SteamVR_Input_Sources.RightHand)) :
                                                             AxisValue(defaultActions.Stationary_Use);
                        break;
                    case InputCommandType.TOOL_UP:
                        __instance.AxisValue = ToolsActive ? AxisValue(toolActions.DPad.axis.y, true) :
                                                             AxisValue(defaultActions.Stationary_DPAD.axis.y, true);
                        break;
                    case InputCommandType.TOOL_DOWN:
                        __instance.AxisValue = ToolsActive ? AxisValue(-toolActions.DPad.axis.y, true):
                                                             AxisValue(-defaultActions.Stationary_DPAD.axis.y, true);
                        break;
                    case InputCommandType.TOOL_RIGHT:
                        __instance.AxisValue = ToolsActive ? AxisValue(toolActions.DPad.axis.x, true):
                                                             AxisValue(defaultActions.Stationary_DPAD.axis.x, true);
                        break;
                    case InputCommandType.TOOL_LEFT:
                        __instance.AxisValue = ToolsActive ? AxisValue(-toolActions.DPad.axis.x, true):
                                                             AxisValue(-defaultActions.Stationary_DPAD.axis.x, true);
                        break;
                }
            }

            public static bool ToolsActive => SteamVR_Actions.tools.IsActive(SteamVR_Input_Sources.RightHand)
                                            || SteamVR_Actions.tools.IsActive(SteamVR_Input_Sources.LeftHand);

            public static void DoRumble(float hiPower, float lowPower)
            {
                hiPower *= 1.42857146f;
                lowPower *= 1.42857146f;
                var haptic = SteamVR_Actions.default_Haptic;
                var frequency = 0.1f;
                var amplitudeY = lowPower * ModSettings.VibrationStrength;
                var amplitudeX = hiPower * ModSettings.VibrationStrength;
                haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.RightHand);
                haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.RightHand);
                haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.LeftHand);
                haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.LeftHand);
            }

            private static void ForceGamepadEnabled(ref bool __result)
            {
                __result = true;
            }
        }
    }
}