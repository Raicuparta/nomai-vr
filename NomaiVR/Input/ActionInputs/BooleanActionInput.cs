using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class BooleanActionInput : ActionInput<SteamVR_Action_Boolean>
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
}