using System.Collections.Generic;
using static InputConsts;

namespace NomaiVR.Input
{
    public static class InputMap
    {
        public static readonly Dictionary<InputCommandType, VRActionInputs.VRActionInput> Map = new Dictionary<InputCommandType, VRActionInputs.VRActionInput>()
        {
            { InputCommandType.JUMP, VRActionInputs.Jump },
            { InputCommandType.BOOST, VRActionInputs.Jump },
            { InputCommandType.SWAP_SHIP_LOG_MODE, VRActionInputs.Jump },
            { InputCommandType.INTERACT_SECONDARY, VRActionInputs.Jump },
            { InputCommandType.MATCH_VELOCITY, VRActionInputs.Jump },
            { InputCommandType.UP, VRActionInputs.UIDpad },
            { InputCommandType.DOWN, VRActionInputs.UIDpad },
            { InputCommandType.RIGHT, VRActionInputs.UIDpad },
            { InputCommandType.MENU_RIGHT, VRActionInputs.UIDpad },
            { InputCommandType.LEFT, VRActionInputs.UIDpad },
            { InputCommandType.MENU_LEFT, VRActionInputs.UIDpad },
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
            { InputCommandType.LOOK_Y, VRActionInputs.Look },
            { InputCommandType.MOVE_XZ, VRActionInputs.Move },
            { InputCommandType.MOVE_X, VRActionInputs.Move },
            { InputCommandType.MOVE_Z, VRActionInputs.Move },
            { InputCommandType.THRUST_UP, VRActionInputs.ThrustUp },
            { InputCommandType.THRUST_DOWN, VRActionInputs.ThrustDown },
        };
    }
}