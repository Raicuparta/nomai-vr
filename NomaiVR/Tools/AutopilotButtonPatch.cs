using System;
using NomaiVR.Assets;
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Tools
{
    internal class AutopilotButtonPatch : NomaiVRModule<AutopilotButtonPatch.AutopilotButton, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class AutopilotButton : MonoBehaviour
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
            private Color textDefaultColor;
            private Animator animator;
            private Text text;
            private Material buttonTopMat;
            private ShipMonitorInteraction interactor;
            private ShipCockpitController cockpitController;
            private AutopilotButtonState buttonState = AutopilotButtonState.PreInit;

            private void Awake()
            {
                var cockpitTech = FindObjectOfType<ShipBody>().transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");
                cockpitController = FindObjectOfType<ShipCockpitController>();
                
                //Add autopilot button
                var autopilotButton = Instantiate(AssetLoader.AutopilotButtonPrefab, transform, false);
                animator = autopilotButton.GetComponentInChildren<Animator>();
                text = autopilotButton.GetComponentInChildren<Text>();
                textDefaultColor = text.color;
                buttonTopMat = autopilotButton.transform.Find("AutoPilot_ButtonTop").GetComponent<Renderer>().material;

                transform.SetParent(cockpitTech, false);
                transform.localPosition = cockpitTech.Find("LandingCamScreen").transform.localPosition + new Vector3(-0.93f, 3.6f, 5.2f);
                transform.localRotation = Quaternion.Euler(45, 180, 0);
                interactor = gameObject.AddComponent<ShipMonitorInteraction>();
                interactor.text = UITextType.ShipAutopilotPrompt;
                interactor.button = InputConsts.InputCommandType.AUTOPILOT;
                interactor.SkipPressCallback = OnPress;

                SetState(AutopilotButtonState.Off);
            }

            private void Update()
            {
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
                if (IsAutopilotActive() && interactor.text != UITextType.ShipAbortAutopilotPrompt)
                {
                    interactor.text = UITextType.ShipAbortAutopilotPrompt;
                    interactor.receiver?.SetPromptText(interactor.text);
                }

                if (!IsAutopilotActive() && interactor.text != UITextType.ShipAutopilotPrompt)
                {
                    interactor.text = UITextType.ShipAutopilotPrompt;
                    interactor.receiver?.SetPromptText(interactor.text);
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

            private void SetButtonColor(Color color)
            {
                buttonTopMat.SetColor(emissionColor, color);
            }

            private bool OnPress()
            {
                animator.SetTrigger(pressAnimation);
                return false;
            }
        }
    }
}