using System.Collections.Generic;
using NomaiVR.Helpers;
using NomaiVR.Input.ActionInputs;
using NomaiVR.Tools;
using NomaiVR.UI;
using Valve.VR;
using static InputConsts;
using static Valve.VR.SteamVR_Actions;

namespace NomaiVR.Input
{
    public static class OptionalToolInputs
    {
        private static readonly HandBooleanActionInput ToolUse =
            new HandBooleanActionInput(_default.GripClick, () => InteractingHandSource);

        private static readonly HandBooleanActionInput ToolSecondary =
            new HandBooleanActionInput(tools.Secondary, () => InteractingHandSource);

        private static readonly BooleanActionInput ToolUp =
            new BooleanActionInput(action: tools.DPadUp, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ToolDown =
            new BooleanActionInput(action: tools.DPadDown, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ToolLeft =
            new BooleanActionInput(action: tools.DPadLeft, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ToolRight =
            new BooleanActionInput(action: tools.DPadRight, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ShipToolUse =
            new BooleanActionInput(action: ship_tools.Use, eitherHand: true);

        private static readonly BooleanActionInput ShipToolSecondary =
            new BooleanActionInput(action: ship_tools.Secondary, eitherHand: true);

        private static readonly BooleanActionInput ShipToolUp =
            new BooleanActionInput(action: ship_tools.DPadUp, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ShipToolDown =
            new BooleanActionInput(action: ship_tools.DPadDown, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ShipToolLeft =
            new BooleanActionInput(action: ship_tools.DPadLeft, eitherHand: true, clickable: false);

        private static readonly BooleanActionInput ShipToolRight =
            new BooleanActionInput(action: ship_tools.DPadRight, eitherHand: true, clickable: false);

        private static readonly Dictionary<InputCommandType, IActionInput> ToolsInputMap =
            new Dictionary<InputCommandType, IActionInput>
            {
                { InputCommandType.PROBELAUNCH, ToolUse },
                { InputCommandType.PROBERETRIEVE, ToolUse },
                { InputCommandType.SIGNALSCOPE, ToolUse },
                { InputCommandType.SCOPEVIEW, ToolUse },
                { InputCommandType.TOOL_PRIMARY, ToolUse },
                { InputCommandType.TOOL_SECONDARY, ToolSecondary },
                { InputCommandType.TOOL_UP, ToolUp },
                { InputCommandType.TOOL_DOWN, ToolDown },
                { InputCommandType.TOOL_RIGHT, ToolRight },
                { InputCommandType.TOOL_LEFT, ToolLeft },
            };

        private static readonly Dictionary<InputCommandType, IActionInput> ShipToolsInputMap =
            new Dictionary<InputCommandType, IActionInput>
            {
                { InputCommandType.PROBELAUNCH, ShipToolUse },
                { InputCommandType.PROBERETRIEVE, ShipToolUse },
                { InputCommandType.TOOL_PRIMARY, ShipToolUse },
                { InputCommandType.TOOL_SECONDARY, ShipToolSecondary },
                { InputCommandType.TOOL_UP, ShipToolUp },
                { InputCommandType.TOOL_DOWN, ShipToolDown },
                { InputCommandType.TOOL_RIGHT, ShipToolRight },
                { InputCommandType.TOOL_LEFT, ShipToolLeft },
            };

        public static bool TryGetActionInput(InputCommandType commandType, out IActionInput actionInput)
        {
            actionInput = null;
            if (!IsSteamFrame) return false;

            var inputMap = ToolHelper.IsUsingAnyTool(ToolGroup.Ship) ? ShipToolsInputMap
                : IsHoldingHandheldTool ? ToolsInputMap
                : null;

            return inputMap != null && inputMap.TryGetValue(commandType, out actionInput);
        }

        private static bool IsSteamFrame => InputPrompts.Instance != null
                                            && InputPrompts.Instance.Platform == InputPrompts.ActiveVRPlatform.SteamFrame;

        private static bool IsHoldingHandheldTool => VRToolSwapper.InteractingHand != null
                                                     && InputHelper.IsHandheldTool();

        private static SteamVR_Input_Sources InteractingHandSource =>
            VRToolSwapper.InteractingHand != null ? VRToolSwapper.InteractingHand.InputSource : SteamVR_Input_Sources.Any;
    }
}
