using NomaiVR.Input;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NomaiVR.Tools
{
    internal class AutopilotButtonPatch : NomaiVRModule<AutopilotButtonPatch.AutopilotButton, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class AutopilotButton : MonoBehaviour
        {
            private Animator _animator;
            private Material _buttonTopMat;
            private Color _onColor;
            private ShipMonitorInteraction _interactor;
            private ShipCockpitController _cockpitController;
            private bool _autoPilotLightEnabled = false;

            private void Awake()
            {
                var cockpitTech = FindObjectOfType<ShipBody>().transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");
                _cockpitController = FindObjectOfType<ShipCockpitController>();


                //Add autopilot button
                var autopilotButton = GameObject.Instantiate(AssetLoader.AutopilotButtonPrefab);
                autopilotButton.transform.SetParent(transform, false);

                _animator = autopilotButton.GetComponentInChildren<Animator>();
                _buttonTopMat = autopilotButton.transform.Find("AutoPilot_ButtonTop").GetComponent<Renderer>().material;
                _onColor = _buttonTopMat.GetColor("_EmissionColor");

                transform.SetParent(cockpitTech, false);
                transform.localPosition = cockpitTech.Find("LandingCamScreen").transform.localPosition + new Vector3(-0.93f, 3.6f, 5.2f);
                transform.localRotation = Quaternion.Euler(45, 180, 0);
                _interactor = gameObject.AddComponent<ShipMonitorInteraction>();
                _interactor.text = UITextType.ShipAutopilotPrompt;
                _interactor.button = InputConsts.InputCommandType.AUTOPILOT;
                _interactor.skipPressCallback = OnPress;

                _buttonTopMat.SetColor("_EmissionColor", Color.black);
            }

            private bool OnPress()
            {
                _animator.SetTrigger("Press");
                return false;
            }

            private void Update()
            {
                if (_cockpitController.IsAutopilotAvailable() && !_autoPilotLightEnabled)
                {
                    _autoPilotLightEnabled = true;
                    _buttonTopMat.SetColor("_EmissionColor", _onColor);
                }

                if (!_cockpitController.IsAutopilotAvailable() && _autoPilotLightEnabled)
                {
                    _autoPilotLightEnabled = false;
                    _buttonTopMat.SetColor("_EmissionColor", Color.black);
                }

                UpdatePrompts();
            }

            private void UpdatePrompts()
            {
                if (_cockpitController._autopilot.IsFlyingToDestination()
                    && _interactor.text != UITextType.ShipAbortAutopilotPrompt)
                {
                    _interactor.text = UITextType.ShipAbortAutopilotPrompt;
                    _interactor.receiver?.SetPromptText(_interactor.text);
                }

                if (!_cockpitController._autopilot.IsFlyingToDestination()
                    && _interactor.text != UITextType.ShipAutopilotPrompt)
                {
                    _interactor.text = UITextType.ShipAutopilotPrompt;
                    _interactor.receiver?.SetPromptText(_interactor.text);
                }
            }
        }
    }
}