using NomaiVR.Input;
using UnityEngine;

namespace NomaiVR.Ship
{
    public class ShipProbeButton: MonoBehaviour
    {
        private InputConsts.InputCommandType inputCommandType;

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
            switch (name)
            {
                case "ArrowButtonUp":
                    inputCommandType = InputConsts.InputCommandType.TOOL_UP;
                    return;
                case "ArrowButtonDown":
                    inputCommandType = InputConsts.InputCommandType.TOOL_DOWN;
                    return;
                case "ArrowButtonLeft":
                    inputCommandType = InputConsts.InputCommandType.TOOL_LEFT;
                    return;
                case "ArrowButtonRight":
                    inputCommandType = InputConsts.InputCommandType.TOOL_RIGHT;
                    return;
            }
        }
    }
}