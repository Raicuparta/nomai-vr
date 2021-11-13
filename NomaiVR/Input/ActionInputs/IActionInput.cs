﻿using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public interface IActionInput
    {
        Vector2 Value { get; }
        ISteamVR_Action_In Action { get; }
        bool Optional { get; }
        bool Active { get; }
        SteamVR_Input_Sources ActiveSource { get; }
    }
}