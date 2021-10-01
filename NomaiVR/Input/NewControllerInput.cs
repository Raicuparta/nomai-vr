using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;

namespace NomaiVR.Input
{
    internal class NewControllerInput: NomaiVRModule<NomaiVRModule.EmptyBehaviour, NewControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private static readonly Dictionary<int, bool> simulatedBoolInputs = new Dictionary<int, bool>();

        public static void SimulateInputPress(InputConsts.InputCommandType commandType)
        {
            simulatedBoolInputs[(int)commandType] = true;
        }
        
        public class Patch: NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.UpdateFromAction), nameof(PatchVectorInput));
                Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.UpdateFromAction), nameof(PatchAxisInput));
                Prefix<AbstractCompositeInputCommands<IVectorInputAction>>(nameof(AbstractCompositeInputCommands<IVectorInputAction>.UpdateFromAction), nameof(PatchCompositeVectorInput));
                Prefix<AbstractCompositeInputCommands<IAxisInputAction>>(nameof(AbstractCompositeInputCommands<IAxisInputAction>.UpdateFromAction), nameof(PatchCompositeAxisInput));
                Prefix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchCompositeInput));
                Postfix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchCompositeInput));
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
                var commandTypeKey = (int) commandType;
                if (simulatedBoolInputs.ContainsKey(commandTypeKey) && simulatedBoolInputs[commandTypeKey])
                {
                    __instance.AxisValue = AxisValue(true);
                    simulatedBoolInputs[(int) commandType] = false;
                    return;
                }
                
                var actions = SteamVR_Actions._default;
                switch (commandType)
                {
                    case InputConsts.InputCommandType.JUMP:
                        __instance.AxisValue = AxisValue(actions.Jump);
                        break;
                    case InputConsts.InputCommandType.ENTER:
                    case InputConsts.InputCommandType.SELECT:
                        __instance.AxisValue = AxisValue(actions.UISelect);
                        break;
                    case InputConsts.InputCommandType.UP:
                        __instance.AxisValue = actions.Move.axis;
                        break;
                    case InputConsts.InputCommandType.DOWN:
                        __instance.AxisValue = -actions.Move.axis;
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
                        __instance.AxisValue = AxisValue(actions.Interact);
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
                }
            }
        }
    }
}