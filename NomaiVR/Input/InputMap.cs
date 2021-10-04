using System.Collections.Generic;
using Valve.VR;
using static InputConsts;
using static Valve.VR.SteamVR_Actions;

namespace NomaiVR.Input
{
    public static class InputMap
    {
        public class VRActionInput
        {
            public ISteamVR_Action Action;
        }
        
        public static readonly Dictionary<InputCommandType, VRActionInput> Map = new Dictionary<InputCommandType, VRActionInput>()
        {
            {
                InputCommandType.JUMP, 
                new VRActionInput() { Action = _default.Jump }
            },
            {
                InputCommandType.BOOST, 
                new VRActionInput() { Action = _default.Jump }
            },
            {
                InputCommandType.SWAP_SHIP_LOG_MODE,
                new VRActionInput() { Action = _default.Jump }
            },
            {
                InputCommandType.INTERACT_SECONDARY,
                new VRActionInput() { Action = _default.Jump }
            },
            {
                InputCommandType.MATCH_VELOCITY,
                new VRActionInput() { Action = _default.Jump }
            },
            {
                InputCommandType.UP,
                new VRActionInput() { Action = _default.UIDpad }
            },
            {
                InputCommandType.DOWN,
                new VRActionInput() { Action = _default.UIDpad }
            },
            {
                InputCommandType.RIGHT,
                new VRActionInput() { Action = _default.UIDpad }
            },
            {
                InputCommandType.MENU_RIGHT,
                new VRActionInput() { Action = _default.UIDpad }
            },
            {
                InputCommandType.LEFT,
                new VRActionInput() { Action = _default.UIDpad }
            },
            {
                InputCommandType.MENU_LEFT,
                new VRActionInput() { Action = _default.UIDpad }
            },
            {
                InputCommandType.TABR,
                new VRActionInput() { Action = _default.UITabRight }
            },
            {
                InputCommandType.TABL,
                new VRActionInput() { Action = _default.UITabLeft }
            },
            {
                InputCommandType.SUBMENU_RIGHT,
                new VRActionInput() { Action = _default.UISubtabRight }
            },
            {
                InputCommandType.SUBMENU_LEFT,
                new VRActionInput() { Action = _default.UISubtabLeft }
            },
            {
                InputCommandType.MAP,
                new VRActionInput() { Action = _default.Map }
            },
            {
                InputCommandType.PAUSE,
                new VRActionInput() { Action = _default.Menu }
            },
            {
                InputCommandType.CONFIRM,
                new VRActionInput() { Action = _default.Menu }
            },
            {
                InputCommandType.ESCAPE,
                new VRActionInput() { Action = _default.Back }
            },
            {
                InputCommandType.CANCEL,
                new VRActionInput() { Action = _default.Back }
            },
            {
                InputCommandType.SELECT,
                new VRActionInput() { Action = _default.Interact }
            },
            {
                InputCommandType.INTERACT,
                new VRActionInput() { Action = _default.Interact }
            },
            {
                InputCommandType.LOCKON,
                new VRActionInput() { Action = _default.Interact }
            },
            {
                InputCommandType.ENTER,
                new VRActionInput() { Action = _default.Interact }
            },
            {
                InputCommandType.LOOK,
                new VRActionInput() { Action = _default.Look }
            },
            {
                InputCommandType.LOOK_X,
                new VRActionInput() { Action = _default.Look }
            },
            {
                InputCommandType.LOOK_Y,
                new VRActionInput() { Action = _default.Look }
            },
            {
                InputCommandType.MOVE_XZ,
                new VRActionInput() { Action = _default.Move }
            },
            {
                InputCommandType.MOVE_X,
                new VRActionInput() { Action = _default.Move }
            },
            {
                InputCommandType.MOVE_Z,
                new VRActionInput() { Action = _default.Move }
            },
            {
                InputCommandType.THRUST_UP,
                new VRActionInput() { Action = _default.ThrustUp }
            },
            {
                InputCommandType.THRUST_DOWN,
                new VRActionInput() { Action = _default.ThrustDown }
            },
        };
    }
}