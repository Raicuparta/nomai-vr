using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class BooleanActionInput : ActionInput<SteamVR_Action_Boolean>
    {
        private readonly bool isEitherHand;
        private readonly bool clickable;

        public override Vector2 Value
        {
            get
            {
                var state = isEitherHand
                    ? SpecificAction.GetState(SteamVR_Input_Sources.LeftHand) ||
                      SpecificAction.GetState(SteamVR_Input_Sources.RightHand)
                    : SpecificAction.state;
                return new Vector2(state ? 1f : 0f, 0f);
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

        public bool Clickable => clickable;

        public BooleanActionInput(SteamVR_Action_Boolean action, bool eitherHand = false, bool clickable = true) : base(action)
        {
            isEitherHand = eitherHand;
            this.clickable = clickable;
        }
    }
}