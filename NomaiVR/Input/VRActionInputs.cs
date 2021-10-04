using System;
using UnityEngine;
using Valve.VR;
using static Valve.VR.SteamVR_Actions;

namespace NomaiVR.Input
{
    public static class VRActionInputs
    {
        public interface IVRActionInput
        {
            Vector2 Value { get; } 
            ISteamVR_Action Action { get; }
        }
        
        public abstract class VRActionInput<TAction> : IVRActionInput where TAction : ISteamVR_Action
        {
            public TAction SpecificAction;
            public ISteamVR_Action Action => SpecificAction;
            
            public abstract Vector2 Value { get; }
        }
        
        public class VRBooleanActionInput : VRActionInput<SteamVR_Action_Boolean>
        {
            public bool IsEitherHand;

            public override Vector2 Value
            {
                get
                {
                    var state = IsEitherHand
                        ? SpecificAction.GetState(SteamVR_Input_Sources.LeftHand) ||
                          SpecificAction.GetState(SteamVR_Input_Sources.RightHand)
                        : SpecificAction.state;
                    return new Vector2(state ? 1f : 0f, 0f);
                }
            }
        }
        
        public class VRSingleActionInput : VRActionInput<SteamVR_Action_Single>
        {
            public override Vector2 Value =>
                 new Vector2(SpecificAction.axis, 0f);
        }
        
        public class VRVector2ActionInput : VRActionInput<SteamVR_Action_Vector2>
        {
            private readonly bool yOnly;
            private readonly bool invert;
            private readonly bool clamp;

            public VRVector2ActionInput(bool yOnly = false, bool invert = false, bool clamp = false)
            {
                this.yOnly = yOnly;
                this.invert = invert;
                this.clamp = clamp;
            }

            public override Vector2 Value
            {
                get
                {
                    var axis = yOnly ? SpecificAction.axis.y : SpecificAction.axis.x;
                    var rawValue = invert ? -axis : axis;
                    var clampedValue = clamp ? Mathf.Clamp(rawValue, 0f, 1f) : rawValue;
                    return new Vector2(clampedValue, yOnly ? 0f : SpecificAction.axis.y);
                }
            }
        }

        public class VREmptyActionInput : VRActionInput<ISteamVR_Action>
        {
            public override Vector2 Value => Vector2.zero;
        }

        public static readonly VREmptyActionInput Empty = new VREmptyActionInput();
        public static readonly VRBooleanActionInput Autopilot = new VRBooleanActionInput() {
            SpecificAction = _default.Autopilot,
        };
        public static readonly VRBooleanActionInput Back = new VRBooleanActionInput() {
            SpecificAction = _default.Back
        };
        public static readonly VRBooleanActionInput Grip = new VRBooleanActionInput() {
            SpecificAction = _default.Grip
        };
        public static readonly VRBooleanActionInput Interact = new VRBooleanActionInput() {
            SpecificAction = _default.Interact
        };
        public static readonly VRBooleanActionInput Jump = new VRBooleanActionInput() {
            SpecificAction = _default.Jump
        };
        public static readonly VRVector2ActionInput Look = new VRVector2ActionInput() {
            SpecificAction = _default.Look
        };
        public static readonly VRVector2ActionInput LookY = new VRVector2ActionInput(yOnly: true) {
            SpecificAction = _default.Look
        };
        public static readonly VRBooleanActionInput Map = new VRBooleanActionInput() {
            SpecificAction = _default.Map
        };
        public static readonly VRBooleanActionInput Menu = new VRBooleanActionInput() {
            SpecificAction = _default.Menu
        };
        public static readonly VRVector2ActionInput Move = new VRVector2ActionInput() {
            SpecificAction = _default.Move
        };
        public static readonly VRVector2ActionInput MoveZ = new VRVector2ActionInput(yOnly: true) {
            SpecificAction = _default.Move
        };
        public static readonly VRBooleanActionInput Recenter = new VRBooleanActionInput() {
            SpecificAction = _default.Recenter
        };
        public static readonly VRBooleanActionInput RollMode = new VRBooleanActionInput() {
            SpecificAction = _default.RollMode
        };
        public static readonly VRBooleanActionInput StationaryUse = new VRBooleanActionInput() {
            SpecificAction = _default.StationaryUse
        };
        public static readonly VRVector2ActionInput StationaryUp = new VRVector2ActionInput(yOnly: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRVector2ActionInput StationaryDown = new VRVector2ActionInput(yOnly: true, invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRVector2ActionInput StationaryLeft = new VRVector2ActionInput(invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRVector2ActionInput StationaryRight = new VRVector2ActionInput(clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRSingleActionInput ThrustDown = new VRSingleActionInput() {
            SpecificAction = _default.ThrustDown
        };
        public static readonly VRSingleActionInput ThrustUp = new VRSingleActionInput() {
            SpecificAction = _default.ThrustUp
        };
        public static readonly VRVector2ActionInput Up = new VRVector2ActionInput(yOnly: true, clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly VRVector2ActionInput Down = new VRVector2ActionInput(yOnly: true, invert: true, clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly VRVector2ActionInput Left = new VRVector2ActionInput(invert: true, clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly VRVector2ActionInput Right = new VRVector2ActionInput(clamp: true) {
            SpecificAction = _default.UIDpad
        };
        public static readonly VRBooleanActionInput UISelect = new VRBooleanActionInput() {
            SpecificAction = _default.UISelect
        };
        public static readonly VRBooleanActionInput UISubtabLeft = new VRBooleanActionInput() {
            SpecificAction = _default.UISubtabLeft
        };
        public static readonly VRBooleanActionInput UISubtabRight = new VRBooleanActionInput() {
            SpecificAction = _default.UISubtabRight
        };
        public static readonly VRBooleanActionInput UITabLeft = new VRBooleanActionInput() {
            SpecificAction = _default.UITabLeft
        };
        public static readonly VRBooleanActionInput UITabRight = new VRBooleanActionInput() {
            SpecificAction = _default.UITabRight
        };
        public static readonly VRBooleanActionInput ToolUse = new VRBooleanActionInput() {
            SpecificAction = tools.Use,
            IsEitherHand = true
        };
        public static readonly VRVector2ActionInput ToolUp = new VRVector2ActionInput(yOnly: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRVector2ActionInput ToolDown = new VRVector2ActionInput(yOnly: true, invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRVector2ActionInput ToolLeft = new VRVector2ActionInput(invert: true, clamp: true) {
            SpecificAction = tools.DPad
        };
        public static readonly VRVector2ActionInput ToolRight = new VRVector2ActionInput(clamp: true) {
            SpecificAction = tools.DPad
        };
    }
}
