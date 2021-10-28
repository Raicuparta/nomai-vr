using System;
using NomaiVR.Input;
using UnityEngine;
using static InputConsts;

namespace NomaiVR.Ship
{
    public class ShipProbeButton: MonoBehaviour
    {
        private InputCommandType inputCommandType;

        private void Awake()
        {
            SetUpInputCommandType();
            var receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(UITextType.ProbeRotatePrompt);
            receiver.OnPressInteract += () =>
            {
                ControllerInput.SimulateInput(inputCommandType, true);
            };
            receiver.OnReleaseInteract += () =>
            {
                ControllerInput.SimulateInput(inputCommandType, false);
                receiver.ResetInteraction();
            };
        }

        private void SetUpInputCommandType()
        {
            inputCommandType = (InputCommandType) Enum.Parse(typeof(InputCommandType), name);
        }
    }
}