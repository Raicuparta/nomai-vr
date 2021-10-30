using System;
using NomaiVR.Helpers;
using NomaiVR.Input;
using UnityEngine;
using static InputConsts;

namespace NomaiVR.Ship
{
    public class ShipProbeButton : MonoBehaviour
    {
        private enum ButtonState
        {
            PreInit,
            Disabled,
            Enabled,
            Focused,
            Active,
        }

        private static readonly int shaderColor = Shader.PropertyToID("_Color");
        private static readonly Color enabledColor = new Color(2.12f, 1.57f, 1.33f, 0.04f);
        private static readonly Color activeColor = new Color(2.11f, 1.67f, 1.33f, 0.2f);
        private static readonly Color disabledColor = new Color(0, 0, 0, 0);
        private static readonly Color hoverColor = new Color(2.12f, 1.67f, 1.33f, 0.1f);
        private ButtonState buttonState = ButtonState.PreInit;
        private Material buttonMaterial;
        private Collider collider;
        private InteractReceiver receiver;
        private InputCommandType inputCommandType;
        private UITextType promptText;

        private void Awake()
        {
            SetUpCommands();
            SetUpReceiver();
            buttonMaterial = GetComponent<Renderer>().material;
            collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (ToolHelper.IsInToolMode(ToolMode.Probe, ToolGroup.Ship))
            {
                if (receiver._isInteractPressed)
                {
                    SetState(ButtonState.Active);
                }
                else if (receiver.IsFocused())
                {
                    SetState(ButtonState.Focused);
                }
                else
                {
                    SetState(ButtonState.Enabled);
                }
            }
            else
            {
                SetState(ButtonState.Disabled);
            }
        }

        private void OnDestroy()
        {
            receiver.OnPressInteract -= OnPressInteract;
            receiver.OnReleaseInteract -= OnReleaseInteract;
            receiver.OnLoseFocus -= OnReleaseInteract;
        }

        private void SetUpReceiver()
        {
            receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(promptText);
            receiver.OnPressInteract += OnPressInteract;
            receiver.OnReleaseInteract += OnReleaseInteract;
            receiver.OnLoseFocus += OnReleaseInteract;
        }

        private void OnPressInteract()
        {
            ControllerInput.SimulateInput(inputCommandType, true);
        }

        private void OnReleaseInteract()
        {
            ControllerInput.SimulateInput(inputCommandType, false);
            receiver.ResetInteraction();
        }

        private void SetUpCommands()
        {
            switch (name)
            {
                case "Up":
                    inputCommandType = InputCommandType.TOOL_UP;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Down":
                    inputCommandType = InputCommandType.TOOL_DOWN;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Left":
                    inputCommandType = InputCommandType.TOOL_LEFT;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Right":
                    inputCommandType = InputCommandType.TOOL_RIGHT;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Shoot":
                    inputCommandType = InputCommandType.TOOL_PRIMARY;
                    promptText = UITextType.ProbeLaunchPrompt;
                    break;
                case "Retrieve":
                    inputCommandType = InputCommandType.PROBERETRIEVE;
                    promptText = UITextType.ProbeRetrievePrompt;
                    break;
            }
        }

        private void SetButtonColor(Color color)
        {
            buttonMaterial.SetColor(shaderColor, color);
        }

        private void SetState(ButtonState nextState, ButtonState? previousState = null)
        {
            if (nextState == buttonState) return;
            if (previousState != null && previousState != buttonState) return;

            collider.enabled = nextState != ButtonState.Disabled;
            switch (nextState)
            {
                case ButtonState.Disabled:
                    SetButtonColor(disabledColor);
                    break;
                case ButtonState.Focused:
                    SetButtonColor(hoverColor);
                    break;
                case ButtonState.Active:
                    SetButtonColor(activeColor);
                    break;
                default:
                    SetButtonColor(enabledColor);
                    break;
            }

            buttonState = nextState;
        }
    }
}