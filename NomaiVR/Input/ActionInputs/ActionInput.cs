using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public abstract class ActionInput<TAction> : IActionInput where TAction : ISteamVR_Action
    {
        public TAction SpecificAction;
        public ISteamVR_Action Action => SpecificAction;

        public abstract Vector2 Value { get; }
    }
}