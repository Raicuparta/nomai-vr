using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public interface IActionInput
    {
        Vector2 Value { get; }
        ISteamVR_Action Action { get; }
    }
}