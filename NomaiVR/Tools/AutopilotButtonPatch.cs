using System;
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

            private static readonly int _pressAnimation = Animator.StringToHash("Press");
            private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");
            private static readonly Color _onColor = new Color(0.19f,0.34f,0.63f);
            private static readonly Color _offColor =  Color.black;
            private static readonly Color _activeColor = new Color(0.99f,0.65f,0.44f);
            private Color _textDefaultColor;
            private Animator _animator;
            private Text _text;
            private Material _buttonTopMat;
            private ShipMonitorInteraction _interactor;
            private ShipCockpitController _cockpitController;
            private AutopilotButtonState _buttonState = AutopilotButtonState.PreInit;

            private void Awake()
            {
                var cockpitTech = FindObjectOfType<ShipBody>().transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");
                _cockpitController = FindObjectOfType<ShipCockpitController>();
                
                //Add autopilot button
                var autopilotButton = Instantiate(AssetLoader.AutopilotButtonPrefab, transform, false);
                _animator = autopilotButton.GetComponentInChildren<Animator>();
                _text = autopilotButton.GetComponentInChildren<Text>();
                _textDefaultColor = _text.color;
                _buttonTopMat = autopilotButton.transform.Find("AutoPilot_ButtonTop").GetComponent<Renderer>().material;

                transform.SetParent(cockpitTech, false);
                transform.localPosition = cockpitTech.Find("LandingCamScreen").transform.localPosition + new Vector3(-0.93f, 3.6f, 5.2f);
                transform.localRotation = Quaternion.Euler(45, 180, 0);
                _interactor = gameObject.AddComponent<ShipMonitorInteraction>();
                _interactor.text = UITextType.ShipAutopilotPrompt;
                _interactor.button = InputConsts.InputCommandType.AUTOPILOT;
                _interactor.skipPressCallback = OnPress;

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
                else if (_cockpitController.IsAutopilotAvailable())
                {
                    SetState(AutopilotButtonState.Available);
                }
                else if (!_cockpitController.IsAutopilotAvailable())
                {
                    SetState(AutopilotButtonState.Off);
                }
            }
            
            private void UpdatePrompts()
            {
                if (IsAutopilotActive() && _interactor.text != UITextType.ShipAbortAutopilotPrompt)
                {
                    _interactor.text = UITextType.ShipAbortAutopilotPrompt;
                    _interactor.receiver?.SetPromptText(_interactor.text);
                }

                if (!IsAutopilotActive() && _interactor.text != UITextType.ShipAutopilotPrompt)
                {
                    _interactor.text = UITextType.ShipAutopilotPrompt;
                    _interactor.receiver?.SetPromptText(_interactor.text);
                }
            }

            private bool IsAutopilotActive()
            {
                return _cockpitController && _cockpitController._autopilot.IsFlyingToDestination();
            }

            private void SetState(AutopilotButtonState state)
            {
                if (state == _buttonState) return;
                switch (state)
                {
                    case AutopilotButtonState.Active:
                        SetButtonColor(_activeColor);
                        _text.color = Color.black;
                        break;
                    case AutopilotButtonState.Available:
                        SetButtonColor(_onColor);
                        _text.color = Color.white;
                        break;
                    default:
                        SetButtonColor(_offColor);
                        _text.color = _textDefaultColor;
                        break;
                }
                _buttonState = state;
            }

            private void SetButtonColor(Color color)
            {
                _buttonTopMat.SetColor(_emissionColor, color);
            }

            private bool OnPress()
            {
                _animator.SetTrigger(_pressAnimation);
                return false;
            }
        }
    }
}