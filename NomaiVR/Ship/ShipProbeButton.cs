using System;
using NomaiVR.Input;
using UnityEngine;
using static InputConsts;

namespace NomaiVR.Ship
{
    public class ShipProbeButton: MonoBehaviour
    {
        private enum ButtonState
        {
            PreInit,
            Disabled,
            Enabled,
            Hover,
            Active,
        }
        
        private static readonly int shaderColor = Shader.PropertyToID("_Color");
        private static readonly Color enabledColor = new Color(2.12f,1.57f,1.33f,0.04f);
        private static readonly Color activeColor = new Color(2.11f,1.67f,1.33f,0.2f);
        private static readonly Color disabledColor = new Color(0.6f,0.6f,0.6f,0.46f);
        private static readonly Color hoverColor =  new Color(2.12f,1.67f,1.33f,0.1f);
        private ButtonState buttonState = ButtonState.PreInit;
        private Material buttonMaterial;
        
        private void Awake()
        {
            var inputCommandType = GetInputCommandType();
            SetUpReceiver(inputCommandType);
            buttonMaterial = GetComponent<Renderer>().material;
            SetState(ButtonState.Disabled);
        }

        private InputCommandType GetInputCommandType()
        {
            return (InputCommandType) Enum.Parse(typeof(InputCommandType), name);
        }

        private void SetUpReceiver(InputCommandType inputCommandType)
        {
            var receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(UITextType.ProbeRotatePrompt);
            receiver.OnPressInteract += () =>
            {
                ControllerInput.SimulateInput(inputCommandType, true);
                SetState(ButtonState.Active);
            };
            receiver.OnReleaseInteract += () =>
            {
                ControllerInput.SimulateInput(inputCommandType, false);
                receiver.ResetInteraction();
                SetState(ButtonState.Enabled);
            };
            receiver.OnGainFocus += () =>
            {
                SetState(ButtonState.Hover);
            };
            receiver.OnLoseFocus += () =>
            {
                ControllerInput.SimulateInput(inputCommandType, false);
                receiver.ResetInteraction();
                SetState(ButtonState.Enabled);
            };
        }
        
        private void SetButtonColor(Color color)
        {
            buttonMaterial.SetColor(shaderColor, color);
        }
        
        private void SetState(ButtonState state)
        {
            if (state == buttonState) return;
            switch (state)
            {
                case ButtonState.Disabled:
                    SetButtonColor(disabledColor);
                    break;
                case ButtonState.Hover:
                    SetButtonColor(hoverColor);
                    break;
                case ButtonState.Active:
                    SetButtonColor(activeColor);
                    break;
                default:
                    SetButtonColor(enabledColor);
                    break;
            }
            buttonState = state;
        }
    }
}