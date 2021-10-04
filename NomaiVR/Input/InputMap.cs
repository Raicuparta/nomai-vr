﻿using System.Collections.Generic;
using NomaiVR.Input.ActionInputs;
using static InputConsts;

namespace NomaiVR.Input
{
    public static class InputMap
    {
        public static readonly Dictionary<InputCommandType, IActionInput> DefaultInputMap =
            new Dictionary<InputCommandType, IActionInput>()
            {
                { InputCommandType.JUMP, ActionInputDefinitions.Jump },
                { InputCommandType.BOOST, ActionInputDefinitions.Jump },
                { InputCommandType.SWAP_SHIP_LOG_MODE, ActionInputDefinitions.Jump },  // This isn't used actually.
                { InputCommandType.MATCH_VELOCITY, ActionInputDefinitions.Jump },
                { InputCommandType.INTERACT_SECONDARY, ActionInputDefinitions.Grip }, // TODO: Temporary, need to find something better.
                { InputCommandType.TOOL_SECONDARY, ActionInputDefinitions.Grip },
                { InputCommandType.AUTOPILOT, ActionInputDefinitions.Autopilot },
                { InputCommandType.ROLL_MODE, ActionInputDefinitions.RollMode },
                { InputCommandType.UP, ActionInputDefinitions.Up },
                { InputCommandType.DOWN, ActionInputDefinitions.Down },
                { InputCommandType.RIGHT, ActionInputDefinitions.Right },
                { InputCommandType.MENU_RIGHT, ActionInputDefinitions.Right },
                { InputCommandType.LEFT, ActionInputDefinitions.Left },
                { InputCommandType.MENU_LEFT, ActionInputDefinitions.Left },
                { InputCommandType.TABR, ActionInputDefinitions.UITabRight },
                { InputCommandType.TABL, ActionInputDefinitions.UITabLeft },
                { InputCommandType.SUBMENU_RIGHT, ActionInputDefinitions.UISubtabRight },
                { InputCommandType.SUBMENU_LEFT, ActionInputDefinitions.UISubtabLeft },
                { InputCommandType.MAP, ActionInputDefinitions.Map },
                { InputCommandType.PAUSE, ActionInputDefinitions.Menu },
                { InputCommandType.CONFIRM, ActionInputDefinitions.Menu },
                { InputCommandType.ESCAPE, ActionInputDefinitions.Back },
                { InputCommandType.CANCEL, ActionInputDefinitions.Back },
                { InputCommandType.SELECT, ActionInputDefinitions.Interact },
                { InputCommandType.INTERACT, ActionInputDefinitions.Interact },
                { InputCommandType.LOCKON, ActionInputDefinitions.Interact },
                { InputCommandType.ENTER, ActionInputDefinitions.Interact },
                { InputCommandType.LOOK, ActionInputDefinitions.Look },
                { InputCommandType.LOOK_X, ActionInputDefinitions.Look },
                { InputCommandType.LOOK_Y, ActionInputDefinitions.LookY },
                { InputCommandType.MOVE_XZ, ActionInputDefinitions.Move },
                { InputCommandType.MOVE_X, ActionInputDefinitions.Move },
                { InputCommandType.MOVE_Z, ActionInputDefinitions.MoveZ },
                { InputCommandType.THRUST_UP, ActionInputDefinitions.ThrustUp },
                { InputCommandType.THRUST_DOWN, ActionInputDefinitions.ThrustDown },
                { InputCommandType.PROBELAUNCH, ActionInputDefinitions.StationaryUse },
                { InputCommandType.PROBERETRIEVE, ActionInputDefinitions.StationaryUse },
                { InputCommandType.SCOPEVIEW, ActionInputDefinitions.StationaryUse },
                { InputCommandType.TOOL_PRIMARY, ActionInputDefinitions.StationaryUse },
                { InputCommandType.TOOL_UP, ActionInputDefinitions.StationaryUp },
                { InputCommandType.TOOL_DOWN, ActionInputDefinitions.StationaryDown },
                { InputCommandType.TOOL_RIGHT, ActionInputDefinitions.StationaryRight },
                { InputCommandType.TOOL_LEFT, ActionInputDefinitions.StationaryLeft },
            };

        public static readonly Dictionary<InputCommandType, IActionInput> ToolsInputMap =
            new Dictionary<InputCommandType, IActionInput>()
            {
                { InputCommandType.PROBELAUNCH, ActionInputDefinitions.ToolUse },
                { InputCommandType.PROBERETRIEVE, ActionInputDefinitions.ToolUse },
                { InputCommandType.SCOPEVIEW, ActionInputDefinitions.ToolUse },
                { InputCommandType.TOOL_PRIMARY, ActionInputDefinitions.ToolUse },
                { InputCommandType.TOOL_UP, ActionInputDefinitions.ToolUp },
                { InputCommandType.TOOL_DOWN, ActionInputDefinitions.ToolDown },
                { InputCommandType.TOOL_RIGHT, ActionInputDefinitions.ToolRight },
                { InputCommandType.TOOL_LEFT, ActionInputDefinitions.ToolLeft },
                { InputCommandType.INTERACT, ActionInputDefinitions.Empty },
            };
        
        
        public static readonly Dictionary<InputCommandType, IActionInput> UsableItemMap =
            new Dictionary<InputCommandType, IActionInput>()
            {
                { InputCommandType.TOOL_PRIMARY, ActionInputDefinitions.Back },
            };
    }
}