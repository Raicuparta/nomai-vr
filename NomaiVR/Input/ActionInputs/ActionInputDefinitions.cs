using static Valve.VR.SteamVR_Actions;

namespace NomaiVR.Input.ActionInputs
{
    public static class ActionInputDefinitions
    {
        public static readonly EmptyActionInput Empty =
            new EmptyActionInput();

        public static readonly EmptyActionInput HoldHands =
            new EmptyActionInput(texturePath: "interactions/hold_together");

        public static readonly BooleanActionInput Autopilot =
            new BooleanActionInput(action: _default.Autopilot);

        public static readonly BooleanActionInput Back =
            new BooleanActionInput(action: _default.Back);

        public static readonly BooleanActionInput Grip =
            new BooleanActionInput(action: _default.Grip);

        public static readonly BooleanActionInput Interact =
            new BooleanActionInput(action: _default.Interact);

        public static readonly BooleanActionInput Jump =
            new BooleanActionInput(action: _default.Jump);

        public static readonly Vector2ActionInput Look =
            new Vector2ActionInput(action: _default.Look);

        public static readonly Vector2ActionInput LookY =
            new Vector2ActionInput(action: _default.Look, yOnly: true);

        public static readonly BooleanActionInput Map =
            new BooleanActionInput(action: _default.Map);

        public static readonly BooleanActionInput Menu =
            new BooleanActionInput(action: _default.Menu);

        public static readonly Vector2ActionInput Move =
            new Vector2ActionInput(action: _default.Move);

        public static readonly Vector2ActionInput MoveZ =
            new Vector2ActionInput(action: _default.Move, yOnly: true);

        public static readonly BooleanActionInput Recenter =
            new BooleanActionInput(action: _default.Recenter);

        public static readonly BooleanActionInput RollMode =
            new BooleanActionInput(action: _default.RollMode);

        public static readonly BooleanActionInput StationaryUse =
            new BooleanActionInput(action: _default.StationaryUse);

        public static readonly Vector2ActionInput StationaryUp =
            new Vector2ActionInput(action: _default.StationaryDpad, yOnly: true, clamp: true, yZero: true, textureModifier: "up");

        public static readonly Vector2ActionInput StationaryDown =
            new Vector2ActionInput(action: _default.StationaryDpad, yOnly: true, invert: true, clamp: true, yZero: true, textureModifier: "down");

        public static readonly Vector2ActionInput StationaryLeft =
            new Vector2ActionInput(action: _default.StationaryDpad, invert: true, clamp: true, yZero: true, textureModifier: "left");

        public static readonly Vector2ActionInput StationaryRight =
            new Vector2ActionInput(action: _default.StationaryDpad, clamp: true, textureModifier: "right");

        public static readonly SingleActionInput ThrustDown =
            new SingleActionInput(action: _default.ThrustDown);

        public static readonly SingleActionInput ThrustUp =
            new SingleActionInput(action: _default.ThrustUp);

        public static readonly Vector2ActionInput Up =
            new Vector2ActionInput(action: _default.UIDpad, yOnly: true, clamp: true, yZero: true, textureModifier: "up");

        public static readonly Vector2ActionInput Down =
            new Vector2ActionInput(action: _default.UIDpad, yOnly: true, invert: true, clamp: true, yZero: true, textureModifier: "down");

        public static readonly Vector2ActionInput Left =
            new Vector2ActionInput(action: _default.UIDpad, invert: true, clamp: true, yZero: true, textureModifier: "left");

        public static readonly Vector2ActionInput Right =
            new Vector2ActionInput(action: _default.UIDpad, clamp: true, yZero: true, textureModifier: "right");

        public static readonly BooleanActionInput UISelect =
            new BooleanActionInput(action: _default.UISelect);

        public static readonly BooleanActionInput UISubtabLeft =
            new BooleanActionInput(action: _default.UISubtabLeft, clickable: false);

        public static readonly BooleanActionInput UISubtabRight =
            new BooleanActionInput(action: _default.UISubtabRight, clickable: false);

        public static readonly BooleanActionInput UITabLeft =
            new BooleanActionInput(action: _default.UITabLeft, clickable: false);

        public static readonly BooleanActionInput UITabRight =
            new BooleanActionInput(action: _default.UITabRight, clickable: false);

        public static readonly BooleanActionInput ToolUse =
            new BooleanActionInput(action: tools.Use, eitherHand: true);

        public static readonly Vector2ActionInput ToolUp =
            new Vector2ActionInput(action: tools.DPad, yOnly: true, clamp: true, yZero: true, eitherHand: true, textureModifier: "up");

        public static readonly Vector2ActionInput ToolDown =
            new Vector2ActionInput(action: tools.DPad, yOnly: true, invert: true, clamp: true, yZero: true, eitherHand: true, textureModifier: "down");

        public static readonly Vector2ActionInput ToolLeft =
            new Vector2ActionInput(action: tools.DPad, invert: true, clamp: true, yZero: true, eitherHand: true, textureModifier: "left");

        public static readonly Vector2ActionInput ToolRight =
            new Vector2ActionInput(action: tools.DPad, clamp: true, yZero: true, eitherHand: true, textureModifier: "right");
    }
}
