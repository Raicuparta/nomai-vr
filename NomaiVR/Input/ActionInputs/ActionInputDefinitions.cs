using static Valve.VR.SteamVR_Actions;

namespace NomaiVR.Input.ActionInputs
{
    public static class ActionInputDefinitions
    {
        public static readonly EmptyActionInput Empty = new EmptyActionInput();
        public static readonly BooleanActionInput Autopilot = new BooleanActionInput() {
            SpecificAction = _default.Autopilot,
        };
        public static readonly BooleanActionInput Back = new BooleanActionInput() {
            SpecificAction = _default.Back
        };
        public static readonly BooleanActionInput Grip = new BooleanActionInput() {
            SpecificAction = _default.Grip
        };
        public static readonly BooleanActionInput Interact = new BooleanActionInput() {
            SpecificAction = _default.Interact
        };
        public static readonly BooleanActionInput Jump = new BooleanActionInput() {
            SpecificAction = _default.Jump
        };
        public static readonly Vector2ActionInput Look = new Vector2ActionInput() {
            SpecificAction = _default.Look
        };
        public static readonly Vector2ActionInput LookY = new Vector2ActionInput(yOnly: true) {
            SpecificAction = _default.Look
        };
        public static readonly BooleanActionInput Map = new BooleanActionInput() {
            SpecificAction = _default.Map
        };
        public static readonly BooleanActionInput Menu = new BooleanActionInput() {
            SpecificAction = _default.Menu
        };
        public static readonly Vector2ActionInput Move = new Vector2ActionInput() {
            SpecificAction = _default.Move
        };
        public static readonly Vector2ActionInput MoveZ = new Vector2ActionInput(yOnly: true) {
            SpecificAction = _default.Move
        };
        public static readonly BooleanActionInput Recenter = new BooleanActionInput() {
            SpecificAction = _default.Recenter
        };
        public static readonly BooleanActionInput RollMode = new BooleanActionInput() {
            SpecificAction = _default.RollMode
        };
        public static readonly BooleanActionInput StationaryUse = new BooleanActionInput() {
            SpecificAction = _default.StationaryUse
        };
        public static readonly Vector2ActionInput StationaryUp = new Vector2ActionInput(yOnly: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly Vector2ActionInput StationaryDown = new Vector2ActionInput(yOnly: true, invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly Vector2ActionInput StationaryLeft = new Vector2ActionInput(invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly Vector2ActionInput StationaryRight = new Vector2ActionInput(clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly SingleActionInput ThrustDown = new SingleActionInput() {
            SpecificAction = _default.ThrustDown
        };
        public static readonly SingleActionInput ThrustUp = new SingleActionInput() {
            SpecificAction = _default.ThrustUp
        };
        public static readonly Vector2ActionInput Up = new Vector2ActionInput(yOnly: true, clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly Vector2ActionInput Down = new Vector2ActionInput(yOnly: true, invert: true, clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly Vector2ActionInput Left = new Vector2ActionInput(invert: true, clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly Vector2ActionInput Right = new Vector2ActionInput(clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly BooleanActionInput UISelect = new BooleanActionInput() {
            SpecificAction = _default.UISelect
        };
        public static readonly BooleanActionInput UISubtabLeft = new BooleanActionInput() {
            SpecificAction = _default.UISubtabLeft
        };
        public static readonly BooleanActionInput UISubtabRight = new BooleanActionInput() {
            SpecificAction = _default.UISubtabRight
        };
        public static readonly BooleanActionInput UITabLeft = new BooleanActionInput() {
            SpecificAction = _default.UITabLeft
        };
        public static readonly BooleanActionInput UITabRight = new BooleanActionInput() {
            SpecificAction = _default.UITabRight
        };
        public static readonly BooleanActionInput ToolUse = new BooleanActionInput() {
            SpecificAction = tools.Use,
            IsEitherHand = true
        };
        public static readonly Vector2ActionInput ToolUp = new Vector2ActionInput(yOnly: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly Vector2ActionInput ToolDown = new Vector2ActionInput(yOnly: true, invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly Vector2ActionInput ToolLeft = new Vector2ActionInput(invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly Vector2ActionInput ToolRight = new Vector2ActionInput(clamp: true) {
            SpecificAction = tools.DPad
        };
    }
}
