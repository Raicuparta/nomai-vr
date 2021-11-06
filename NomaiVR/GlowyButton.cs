using UnityEngine;

namespace NomaiVR
{
    public abstract class GlowyButton : MonoBehaviour
    {
        public enum ButtonState
        {
            PreInit,
            Disabled,
            Enabled,
            Focused,
            Active,
        }

        private static readonly int shaderColor = Shader.PropertyToID("_Color");
        private static readonly Color disabledColor = new Color(0, 0, 0, 0);
        private static readonly Color enabledColor = new Color(2.12f, 1.57f, 1.33f, 0.04f);
        private static readonly Color hoverColor = new Color(2.12f, 1.67f, 1.33f, 0.1f);
        private static readonly Color activeColor = new Color(2.11f, 1.67f, 1.33f, 0.2f);
        private ButtonState buttonState = ButtonState.PreInit;
        private Material buttonMaterial;
        private Collider collider;

        public ButtonState State => buttonState;

        private void Awake()
        {
            buttonMaterial = GetComponentInChildren<Renderer>().material;
            collider = GetComponent<Collider>();
            Initialize();
        }

        private void Update()
        {
            if (IsButtonEnabled())
            {
                if (IsButtonActive())
                {
                    SetState(ButtonState.Active);
                }
                else if (IsButtonFocused())
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

        protected abstract void Initialize();
        protected abstract bool IsButtonActive();
        protected abstract bool IsButtonFocused();
        protected abstract bool IsButtonEnabled();

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