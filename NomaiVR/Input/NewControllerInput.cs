using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;

namespace NomaiVR.Input
{
    internal class NewControllerInput : NomaiVRModule<NomaiVRModule.EmptyBehaviour, NewControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private static readonly Dictionary<int, bool> simulatedBoolInputs = new Dictionary<int, bool>();

        public static void SimulateInputPress(InputConsts.InputCommandType commandType)
        {
            simulatedBoolInputs[(int)commandType] = true;
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.UpdateFromAction), nameof(PatchVectorInput));
                Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.UpdateFromAction), nameof(PatchAxisInput));
                Prefix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchCompositeInput));
                Postfix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchCompositeInput));
                Prefix<InputManager>(nameof(InputManager.Rumble), nameof(DoRumble));

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

            private static Vector2 AxisValue(float value)
            {
                return new Vector2(value, 0f);
            }

            private static Vector2 AxisValue(ISteamVR_Action_Boolean action)
            {
                return AxisValue(action.state);
            }

            private static Vector2 AxisValue(ISteamVR_Action_Single action)
            {
                return new Vector2(action.axis, 0f);
            }

            private static void PatchCompositeInput(CompositeInputCommands __instance)
            {
                Command(__instance);
            }

            public static void PatchVectorInput(AbstractInputCommands<IVectorInputAction> __instance)
            {
                Command(__instance);
            }

            public static void PatchAxisInput(AbstractInputCommands<IAxisInputAction> __instance)
            {
                Command(__instance);
            }

            public static void PatchCompositeVectorInput(AbstractCompositeInputCommands<IVectorInputAction> __instance)
            {
                Command(__instance);
            }

            public static void PatchCompositeAxisInput(AbstractCompositeInputCommands<IAxisInputAction> __instance)
            {
                Command(__instance);
            }

            private static void Command(AbstractCommands __instance)
            {
                var commandType = __instance.CommandType;
                var commandTypeKey = (int)commandType;
                if (simulatedBoolInputs.ContainsKey(commandTypeKey) && simulatedBoolInputs[commandTypeKey])
                {
                    __instance.AxisValue = AxisValue(true);
                    simulatedBoolInputs[(int)commandType] = false;
                    return;
                }

                var actions = SteamVR_Actions._default;
                var tools = SteamVR_Actions.tools;
                switch (commandType)
                {
                    case InputConsts.InputCommandType.JUMP:
                    case InputConsts.InputCommandType.BOOST:
                        __instance.AxisValue = AxisValue(actions.Jump);
                        break;
                    case InputConsts.InputCommandType.ENTER:
                    case InputConsts.InputCommandType.SELECT:
                        __instance.AxisValue = AxisValue(actions.UISelect);
                        break;
                    case InputConsts.InputCommandType.UP:
                        __instance.AxisValue = AxisValue(Mathf.Clamp(actions.Move.axis.y, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.DOWN:
                        __instance.AxisValue = AxisValue(Mathf.Clamp(-actions.Move.axis.y, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.RIGHT:
                        __instance.AxisValue = AxisValue(Mathf.Clamp(actions.Move.axis.x, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.LEFT:
                        __instance.AxisValue = AxisValue(Mathf.Clamp(-actions.Move.axis.x, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.MAP:
                        __instance.AxisValue = AxisValue(actions.Map);
                        break;
                    case InputConsts.InputCommandType.PAUSE:
                    case InputConsts.InputCommandType.CONFIRM:
                        __instance.AxisValue = AxisValue(actions.Menu);
                        break;
                    case InputConsts.InputCommandType.ESCAPE:
                    case InputConsts.InputCommandType.CANCEL:
                        __instance.AxisValue = AxisValue(actions.Back);
                        break;
                    case InputConsts.InputCommandType.INTERACT:
                    case InputConsts.InputCommandType.LOCKON:
                        __instance.AxisValue = AxisValue(!ToolsActive && actions.Interact.state);
                        break;
                    case InputConsts.InputCommandType.LOOK:
                        __instance.AxisValue = actions.Look.axis;
                        break;
                    case InputConsts.InputCommandType.LOOK_X:
                        __instance.AxisValue = actions.Look.axis;
                        break;
                    case InputConsts.InputCommandType.LOOK_Y:
                        __instance.AxisValue = AxisValue(actions.Look.axis.y);
                        break;
                    case InputConsts.InputCommandType.MOVE_XZ:
                        __instance.AxisValue = actions.Move.axis;
                        break;
                    case InputConsts.InputCommandType.MOVE_X:
                        __instance.AxisValue = actions.Move.axis;
                        break;
                    case InputConsts.InputCommandType.MOVE_Z:
                        __instance.AxisValue = AxisValue(actions.Move.axis.y);
                        break;
                    case InputConsts.InputCommandType.THRUST_UP:
                        __instance.AxisValue = AxisValue(actions.ThrustUp);
                        break;
                    case InputConsts.InputCommandType.THRUST_DOWN:
                        __instance.AxisValue = AxisValue(actions.ThrustDown);
                        break;
                    case InputConsts.InputCommandType.ROLL_MODE:
                        __instance.AxisValue = AxisValue(actions.RollMode);
                        break;
                    case InputConsts.InputCommandType.AUTOPILOT:
                        __instance.AxisValue = AxisValue(actions.Autopilot);
                        break;
                    case InputConsts.InputCommandType.PROBELAUNCH:
                    case InputConsts.InputCommandType.PROBERETRIEVE:
                    case InputConsts.InputCommandType.SCOPEVIEW:
                    case InputConsts.InputCommandType.TOOL_PRIMARY:
                        __instance.AxisValue = ToolsActive ? AxisValue(tools.Use.GetState(SteamVR_Input_Sources.LeftHand) || tools.Use.GetState(SteamVR_Input_Sources.RightHand)) :
                                                             AxisValue(actions.Stationary_Use);
                        break;
                    case InputConsts.InputCommandType.TOOL_UP:
                        __instance.AxisValue = ToolsActive ? AxisValue(Mathf.Clamp(tools.DPad.axis.y, 0f, 1f)) :
                                                             AxisValue(Mathf.Clamp(actions.Stationary_DPAD.axis.y, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.TOOL_DOWN:
                        __instance.AxisValue = ToolsActive ? AxisValue(Mathf.Clamp(-tools.DPad.axis.y, 0f, 1f)):
                                                             AxisValue(Mathf.Clamp(-actions.Stationary_DPAD.axis.y, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.TOOL_RIGHT:
                        __instance.AxisValue = ToolsActive ? AxisValue(Mathf.Clamp(tools.DPad.axis.x, 0f, 1f)):
                                                             AxisValue(Mathf.Clamp(actions.Stationary_DPAD.axis.x, 0f, 1f));
                        break;
                    case InputConsts.InputCommandType.TOOL_LEFT:
                        __instance.AxisValue = ToolsActive ? AxisValue(Mathf.Clamp(-tools.DPad.axis.x, 0f, 1f)):
                                                             AxisValue(Mathf.Clamp(-actions.Stationary_DPAD.axis.x, 0f, 1f));
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
        }
    }
}