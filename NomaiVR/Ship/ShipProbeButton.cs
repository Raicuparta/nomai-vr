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
        private Collider collider;
        private InputCommandType inputCommandType;
        private InteractReceiver receiver;
        
        private void Awake()
        {
            // TODO cleanup events.
            inputCommandType = GetInputCommandType();
            SetUpReceiver();
            buttonMaterial = GetComponent<Renderer>().material;
            collider = GetComponent<Collider>();
            SetInitialState();
            SetUpAnchorEvents();
        }

        private void OnDestroy()
        {
            receiver.OnPressInteract -= OnPressInteract;
            receiver.OnReleaseInteract -= OnReleaseInteract;
            receiver.OnGainFocus -= OnGainFocus;
            receiver.OnLoseFocus -= OnLoseFocus;
            Locator.GetProbe().OnAnchorProbe -= OnAnchorProbe;
            Locator.GetProbe().OnUnanchorProbe -= OnUnanchorProbe;
            Locator.GetProbe().OnRetrieveProbe -= OnUnanchorProbe;
            Locator.GetProbe().OnProbeDestroyed -= OnUnanchorProbe;
        }

        private void SetInitialState()
        {
            if (inputCommandType != InputCommandType.TOOL_PRIMARY && inputCommandType != InputCommandType.PROBERETRIEVE)
            {
                SetState(ButtonState.Disabled);
            }
            else
            {
                SetState(ButtonState.Enabled);
            }
        }
        
        private void SetUpReceiver()
        {
            receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(UITextType.ProbeRotatePrompt);
            receiver.OnPressInteract += OnPressInteract;
            receiver.OnReleaseInteract += OnReleaseInteract;
            receiver.OnGainFocus += OnGainFocus;
            receiver.OnLoseFocus += OnLoseFocus;
        }

        private void SetUpAnchorEvents()
        {
            Locator.GetProbe().OnAnchorProbe += OnAnchorProbe;
            Locator.GetProbe().OnUnanchorProbe += OnUnanchorProbe;
            Locator.GetProbe().OnRetrieveProbe += OnUnanchorProbe;
            Locator.GetProbe().OnProbeDestroyed += OnUnanchorProbe;
        }

        private void OnPressInteract()
        {
            ControllerInput.SimulateInput(inputCommandType, true);
            SetState(ButtonState.Active);
        }

        private void OnReleaseInteract()
        {
            ControllerInput.SimulateInput(inputCommandType, false);
            receiver.ResetInteraction();
            SetState(ButtonState.Enabled);
        }

        private void OnGainFocus()
        {
            SetState(ButtonState.Hover);
        }
        
        private void OnLoseFocus()
        {
            ControllerInput.SimulateInput(inputCommandType, false);
            receiver.ResetInteraction();
            SetState(ButtonState.Enabled);
        }

        private void OnAnchorProbe()
        {
            if (inputCommandType == InputCommandType.TOOL_PRIMARY) return;
            SetState(ButtonState.Enabled);
        }

        private void OnUnanchorProbe()
        {
            if (inputCommandType == InputCommandType.TOOL_PRIMARY) return;
            SetState(ButtonState.Disabled);
        }
        
        private InputCommandType GetInputCommandType()
        {
            return (InputCommandType) Enum.Parse(typeof(InputCommandType), name);
        }
        
        private void SetButtonColor(Color color)
        {
            buttonMaterial.SetColor(shaderColor, color);
        }
        
        private void SetState(ButtonState state)
        {
            if (state == buttonState) return;
            collider.enabled = state != ButtonState.Disabled;
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