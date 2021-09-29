using System;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Valve.VR;

namespace NomaiVR.Input
{
    internal class NewControllerInput: NomaiVRModule<NewControllerInput.Behaviour, NewControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        public class Behaviour : MonoBehaviour
        {
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

            private static Vector2 AxisValueFromAction(ISteamVR_Action_Boolean action)
            {
               return new Vector2(action.state ? 1f : 0f, 0f);
            }
            private static Vector2 AxisValueFromAction(ISteamVR_Action_Single action)
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
                var actions = SteamVR_Actions._default;
                switch (__instance.CommandType)
                {
                    case InputConsts.InputCommandType.JUMP:
                    case InputConsts.InputCommandType.ENTER:
                    case InputConsts.InputCommandType.CONFIRM:
                        __instance.AxisValue = AxisValueFromAction(actions.Jump);
                        break;
                    case InputConsts.InputCommandType.UP:
                        __instance.AxisValue = actions.Move.axis;
                        break;
                    case InputConsts.InputCommandType.DOWN:
                        __instance.AxisValue = -actions.Move.axis;
                        break;
                    case InputConsts.InputCommandType.MAP:
                        __instance.AxisValue = AxisValueFromAction(actions.Map);
                        break;
                    case InputConsts.InputCommandType.PAUSE:
                        __instance.AxisValue = AxisValueFromAction(actions.Menu);
                        break;
                    case InputConsts.InputCommandType.ESCAPE:
                    case InputConsts.InputCommandType.CANCEL:
                        __instance.AxisValue = AxisValueFromAction(actions.Back);
                        break;
                    case InputConsts.InputCommandType.INTERACT:
                        __instance.AxisValue = AxisValueFromAction(actions.Interact);
                        break;
                    case InputConsts.InputCommandType.LOOK:
                        __instance.AxisValue = actions.Look.axis;
                        break;
                    case InputConsts.InputCommandType.LOOK_X:
                        __instance.AxisValue = actions.Look.axis;
                        break;
                    case InputConsts.InputCommandType.LOOK_Y:
                        __instance.AxisValue = new Vector2(actions.Look.axis.y, 0);
                        break;
                    case InputConsts.InputCommandType.MOVE_XZ:
                        __instance.AxisValue = actions.Move.axis;
                        break; 
                    case InputConsts.InputCommandType.MOVE_X:
                        __instance.AxisValue = actions.Move.axis;
                        break;
                    case InputConsts.InputCommandType.MOVE_Z:
                        __instance.AxisValue = new Vector2(actions.Move.axis.y, 0);
                        break;
                    case InputConsts.InputCommandType.THRUST_UP:
                        __instance.AxisValue = AxisValueFromAction(actions.ThrustUp);
                        break;
                    case InputConsts.InputCommandType.THRUST_DOWN:
                        __instance.AxisValue = AxisValueFromAction(actions.ThrustDown);
                        break;
                    case InputConsts.InputCommandType.ROLL_MODE:
                        __instance.AxisValue = AxisValueFromAction(actions.RollMode);
                        break;
                }
            }
        }
    }
}