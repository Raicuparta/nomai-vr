using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class Vector2ActionInput : ActionInput<SteamVR_Action_Vector2>
    {
        private readonly bool clamp;
        private readonly bool invert;
        private readonly bool yOnly;
        private readonly bool yZero;
        private readonly bool isEitherHand;

        public Vector2ActionInput(SteamVR_Action_Vector2 action, bool yOnly = false, bool invert = false, bool clamp = false, bool yZero = false, bool eitherHand = false): base(action)
        {
            this.yOnly = yOnly;
            this.invert = invert;
            this.clamp = clamp;
            this.yZero = yZero;
            this.isEitherHand = eitherHand;
        }

        public override Vector2 Value
        {
            get
            {
                var axis = yOnly ? specificAction.axis.y : specificAction.axis.x;
                var rawValue = invert ? -axis : axis;
                var clampedValue = clamp ? Mathf.Clamp(rawValue, 0f, 1f) : rawValue;
                return new Vector2(clampedValue, (yOnly || yZero) ? 0f : specificAction.axis.y);
            }
        }

        public override bool Active
        {
            get
            {
                var state = isEitherHand
                    ? specificAction.GetActive(SteamVR_Input_Sources.LeftHand) ||
                      specificAction.GetActive(SteamVR_Input_Sources.RightHand)
                    : specificAction.active;
                return state;
            }
        }

        public override SteamVR_Input_Sources ActiveSource
        {
            get
            {
                var state = isEitherHand
                    ? (SteamVR_Input_Sources)((int)specificAction.GetActiveDevice(SteamVR_Input_Sources.LeftHand) +
                                              (int)specificAction.GetActiveDevice(SteamVR_Input_Sources.RightHand))
                    : specificAction.activeDevice;
                return state;
            }
        }
    }
}