using NomaiVR.Input;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Ship
{
    public class AutopilotButtonInteract : ShipInteractReceiver
    {
        private enum AutopilotButtonState
        {
            PreInit,
            Off,
            Available,
            Active,
        }

        private static readonly int pressAnimation = Animator.StringToHash("Press");
        private static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly Color onColor = new Color(0.19f,0.34f,0.63f);
        private static readonly Color offColor =  Color.black;
        private static readonly Color activeColor = new Color(0.99f,0.65f,0.44f);
        private const InputConsts.InputCommandType inputCommandType = InputConsts.InputCommandType.AUTOPILOT;
        private Color textDefaultColor;
        private Animator animator;
        private Text text;
        private Material buttonTopMat;
        private ShipCockpitController cockpitController;
        private AutopilotButtonState buttonState = AutopilotButtonState.PreInit;

        protected override UITextType Text => UITextType.ShipAutopilotPrompt;
        protected override GameObject ComponentContainer => gameObject;
        
        protected override void Awake()
        {
            base.Awake();
            cockpitController = FindObjectOfType<ShipCockpitController>();
            
            //Add autopilot button
            animator = GetComponentInChildren<Animator>();
            text = GetComponentInChildren<Text>();
            textDefaultColor = text.color;
            buttonTopMat = transform.Find("AutoPilot_ButtonTop").GetComponent<Renderer>().material;

            transform.localPosition = transform.parent.Find("LandingCamScreen").transform.localPosition + new Vector3(-0.93f, 3.6f, 5.2f);
            transform.localRotation = Quaternion.Euler(45, 180, 0);

            SetState(AutopilotButtonState.Off);
        }

        protected override void OnPress()
        {
            animator.SetTrigger(pressAnimation);
            ControllerInput.SimulateInput(inputCommandType, true);
        }

        protected override void OnRelease()
        {
            ControllerInput.SimulateInput(inputCommandType, false);
        }

        protected override bool ShouldDisable()
        {
            return false;
        }

        private void SetButtonColor(Color color)
        {
            buttonTopMat.SetColor(emissionColor, color);
        }

        protected override void Update()
        {
            base.Update();
            UpdateState();
            UpdatePrompts();
        }

        private void UpdateState()
        {
            if (IsAutopilotActive())
            {
                SetState(AutopilotButtonState.Active);
            }
            else if (cockpitController.IsAutopilotAvailable())
            {
                SetState(AutopilotButtonState.Available);
            }
            else if (!cockpitController.IsAutopilotAvailable())
            {
                SetState(AutopilotButtonState.Off);
            }
        }
        
        private void UpdatePrompts()
        {
            if (!Receiver) return;

            if (IsAutopilotActive() && Receiver._textID != UITextType.ShipAbortAutopilotPrompt)
            {
                Receiver.SetPromptText(UITextType.ShipAbortAutopilotPrompt);
            }

            if (!IsAutopilotActive() && Receiver._textID != UITextType.ShipAutopilotPrompt)
            {
                Receiver.SetPromptText(UITextType.ShipAutopilotPrompt);
            }
        }

        private bool IsAutopilotActive()
        {
            return cockpitController && cockpitController._autopilot.IsFlyingToDestination();
        }

        private void SetState(AutopilotButtonState state)
        {
            if (state == buttonState) return;
            switch (state)
            {
                case AutopilotButtonState.Active:
                    SetButtonColor(activeColor);
                    text.color = Color.black;
                    break;
                case AutopilotButtonState.Available:
                    SetButtonColor(onColor);
                    text.color = Color.white;
                    break;
                default:
                    SetButtonColor(offColor);
                    text.color = textDefaultColor;
                    break;
            }
            buttonState = state;
        }
    }
}