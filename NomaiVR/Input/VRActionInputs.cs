using Valve.VR;
using static Valve.VR.SteamVR_Actions;

namespace NomaiVR.Input
{
    public static class VRActionInputs
    {
        public class VRActionInput
        {
            public ISteamVR_Action Action;
        }

        public static readonly VRActionInput Autopilot = new VRActionInput() {Action = _default.Autopilot};
        public static readonly VRActionInput Back = new VRActionInput() {Action = _default.Back};
        public static readonly VRActionInput Grip = new VRActionInput() {Action = _default.Grip};
        public static readonly VRActionInput Interact = new VRActionInput() {Action = _default.Interact};
        public static readonly VRActionInput Jump = new VRActionInput() {Action = _default.Jump};
        public static readonly VRActionInput Look = new VRActionInput() {Action = _default.Look};
        public static readonly VRActionInput Map = new VRActionInput() {Action = _default.Map};
        public static readonly VRActionInput Menu = new VRActionInput() {Action = _default.Menu};
        public static readonly VRActionInput Move = new VRActionInput() {Action = _default.Move};
        public static readonly VRActionInput Recenter = new VRActionInput() {Action = _default.Recenter};
        public static readonly VRActionInput RollMode = new VRActionInput() {Action = _default.RollMode};
        public static readonly VRActionInput StationaryDpad = new VRActionInput() {Action = _default.StationaryDpad};
        public static readonly VRActionInput StationaryUse = new VRActionInput() {Action = _default.StationaryUse};
        public static readonly VRActionInput ThrustDown = new VRActionInput() {Action = _default.ThrustDown};
        public static readonly VRActionInput ThrustUp = new VRActionInput() {Action = _default.ThrustUp};
        public static readonly VRActionInput UIDpad = new VRActionInput() {Action = _default.UIDpad};
        public static readonly VRActionInput UISelect = new VRActionInput() {Action = _default.UISelect};
        public static readonly VRActionInput UISubtabLeft = new VRActionInput() {Action = _default.UISubtabLeft};
        public static readonly VRActionInput UISubtabRight = new VRActionInput() {Action = _default.UISubtabRight};
        public static readonly VRActionInput UITabLeft = new VRActionInput() {Action = _default.UITabLeft};
        public static readonly VRActionInput UITabRight = new VRActionInput() {Action = _default.UITabRight};
    }
}
