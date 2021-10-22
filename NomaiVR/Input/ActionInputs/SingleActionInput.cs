using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class SingleActionInput : ActionInput<SteamVR_Action_Single>
    {
        public override Vector2 Value => new Vector2(SpecificAction.axis, 0f);

        public override bool Active => SpecificAction.active;

        public override SteamVR_Input_Sources ActiveSource => SpecificAction.activeDevice;

        public SingleActionInput(SteamVR_Action_Single action) : base(action)
        {
        }
    }
}