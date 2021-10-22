using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public abstract class ActionInput<TAction> : IActionInput where TAction : ISteamVR_Action_In
    {
        public ISteamVR_Action_In Action => SpecificAction;
        public abstract Vector2 Value { get; }
        public abstract bool Active { get; }
        public abstract SteamVR_Input_Sources ActiveSource { get; }

        protected TAction SpecificAction;

        protected ActionInput(TAction action)
        {
            SpecificAction = action;
        }
    }
}