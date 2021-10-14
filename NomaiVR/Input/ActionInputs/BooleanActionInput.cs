using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class BooleanActionInput : ActionInput<SteamVR_Action_Boolean>
    {
        private readonly bool isEitherHand;

        public override Vector2 Value
        {
            get
            {
                var state = isEitherHand
                    ? specificAction.GetState(SteamVR_Input_Sources.LeftHand) ||
                      specificAction.GetState(SteamVR_Input_Sources.RightHand)
                    : specificAction.state;
                return new Vector2(state ? 1f : 0f, 0f);
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

        public BooleanActionInput(SteamVR_Action_Boolean action, bool eitherHand = false) : base(action)
        {
            isEitherHand = eitherHand;
        }
    }
}