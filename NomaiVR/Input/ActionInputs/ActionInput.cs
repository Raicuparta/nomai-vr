using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public abstract class ActionInput<TAction> : IActionInput where TAction : ISteamVR_Action_In
    {
        public ISteamVR_Action_In Action => specificAction;
        public abstract Vector2 Value { get; }
        public abstract bool Active { get; }

        protected TAction specificAction;

        protected ActionInput(TAction action)
        {
            specificAction = action;
        }
    }
}