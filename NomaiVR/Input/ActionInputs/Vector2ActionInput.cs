using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class Vector2ActionInput : ActionInput<SteamVR_Action_Vector2>
    {
        private readonly bool clamp;
        private readonly bool invert;
        private readonly bool yOnly;

        public Vector2ActionInput(bool yOnly = false, bool invert = false, bool clamp = false)
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
}