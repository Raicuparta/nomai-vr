using System.Collections.Generic;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.Input
{
    public static class InputMap
    {
        public static readonly Dictionary<InputCommandType, VRActionInputs.IVRActionInput> Map = new Dictionary<InputCommandType, VRActionInputs.IVRActionInput>()
        {
            { InputCommandType.JUMP, VRActionInputs.Jump },
            { InputCommandType.BOOST, VRActionInputs.Jump },
            { InputCommandType.SWAP_SHIP_LOG_MODE, VRActionInputs.Jump },
            { InputCommandType.INTERACT_SECONDARY, VRActionInputs.Jump },
            { InputCommandType.MATCH_VELOCITY, VRActionInputs.Jump },
            { InputCommandType.UP, VRActionInputs.Up },
            { InputCommandType.DOWN, VRActionInputs.Down },
            { InputCommandType.RIGHT, VRActionInputs.Right },
            { InputCommandType.MENU_RIGHT, VRActionInputs.Right },
            { InputCommandType.LEFT, VRActionInputs.Left },
            { InputCommandType.MENU_LEFT, VRActionInputs.Left },
            { InputCommandType.TABR, VRActionInputs.UITabRight },
            { InputCommandType.TABL, VRActionInputs.UITabLeft },
            { InputCommandType.SUBMENU_RIGHT, VRActionInputs.UISubtabRight },
            { InputCommandType.SUBMENU_LEFT, VRActionInputs.UISubtabLeft },
            { InputCommandType.MAP, VRActionInputs.Map },
            { InputCommandType.PAUSE, VRActionInputs.Menu },
            { InputCommandType.CONFIRM, VRActionInputs.Menu },
            { InputCommandType.ESCAPE, VRActionInputs.Back },
            { InputCommandType.CANCEL, VRActionInputs.Back },
            { InputCommandType.SELECT, VRActionInputs.Interact },
            { InputCommandType.INTERACT, VRActionInputs.Interact },
            { InputCommandType.LOCKON, VRActionInputs.Interact },
            { InputCommandType.ENTER, VRActionInputs.Interact },
            { InputCommandType.LOOK, VRActionInputs.Look },
            { InputCommandType.LOOK_X, VRActionInputs.Look },
            { InputCommandType.LOOK_Y, VRActionInputs.LookY },
            { InputCommandType.MOVE_XZ, VRActionInputs.Move },
            { InputCommandType.MOVE_X, VRActionInputs.Move },
            { InputCommandType.MOVE_Z, VRActionInputs.MoveZ },
            { InputCommandType.THRUST_UP, VRActionInputs.ThrustUp },
            { InputCommandType.THRUST_DOWN, VRActionInputs.ThrustDown },
        };
    }
}