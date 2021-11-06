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
        private readonly string textureModifier;

        public Vector2ActionInput(SteamVR_Action_Vector2 action, bool optional = false, bool yOnly = false, bool invert = false, bool clamp = false, bool yZero = false, bool eitherHand = false, string textureModifier = null): base(action, optional)
        {
            this.yOnly = yOnly;
            this.invert = invert;
            this.clamp = clamp;
            this.yZero = yZero;
            this.isEitherHand = eitherHand;
            this.textureModifier = textureModifier;
        }

        public override Vector2 Value
        {
            get
            {
                var axis = yOnly ? SpecificAction.axis.y : SpecificAction.axis.x;
                var rawValue = invert ? -axis : axis;
                var clampedValue = clamp ? Mathf.Clamp(rawValue, 0f, 1f) : rawValue;
                return new Vector2(clampedValue, (yOnly || yZero) ? 0f : SpecificAction.axis.y);
            }
        }

        public override bool Active
        {
            get
            {
                var state = isEitherHand
                    ? SpecificAction.GetActive(SteamVR_Input_Sources.LeftHand) ||
                      SpecificAction.GetActive(SteamVR_Input_Sources.RightHand)
                    : SpecificAction.active;
                return state;
            }
        }

        public override SteamVR_Input_Sources ActiveSource
        {
            get
            {
                var state = isEitherHand
                    ? (SteamVR_Input_Sources)((int)SpecificAction.GetActiveDevice(SteamVR_Input_Sources.LeftHand) +
                                              (int)SpecificAction.GetActiveDevice(SteamVR_Input_Sources.RightHand))
                    : SpecificAction.activeDevice;
                return state;
            }
        }

        public string TextureModifier => this.textureModifier;
    }
}