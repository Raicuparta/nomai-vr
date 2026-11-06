using System;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{

    public class HandBooleanActionInput : BooleanActionInput
    {
        private readonly Func<SteamVR_Input_Sources> hand;
        public override Vector2 Value => new Vector2(SpecificAction.GetState(hand()) ? 1f : 0f, 0f);
        public override bool Active => SpecificAction.GetActive(hand());
        public override SteamVR_Input_Sources ActiveSource => hand();
        public HandBooleanActionInput(SteamVR_Action_Boolean action, Func<SteamVR_Input_Sources> hand) : base(action)
        {
            this.hand = hand;
        }
    }
}
