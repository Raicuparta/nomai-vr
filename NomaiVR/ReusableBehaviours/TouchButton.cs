using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.Tools;
using System;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.ReusableBehaviours
{
    public class TouchButton : GlowyButton
    {
        public Func<bool> CheckEnabled { get; set; }
        private float interactRadius = 0.1f;
        private Vector3 interactOffset = Vector3.zero;
        private ProximityDetector proximityDetector;
        private bool isFingertipInside = false;
        private InputCommandType inputToSimulate;

        protected override void Initialize()
        {
            var localSphereCollider = GetComponent<SphereCollider>();
            if (localSphereCollider != null)
            {
                interactRadius = localSphereCollider.radius;
                interactOffset = localSphereCollider.center;
            }

            ResetInputs();
        }

        public void ResetInputs()
        {
            switch (name)
            {
                case "Up":
                    inputToSimulate = InputCommandType.TOOL_UP;
                    break;
                case "Down":
                    inputToSimulate = InputCommandType.TOOL_DOWN;
                    break;
                case "Left":
                case "Camera":
                    inputToSimulate = InputCommandType.TOOL_LEFT;
                    break;
                case "Right":
                    inputToSimulate = InputCommandType.TOOL_RIGHT;
                    break;
                default:
                    inputToSimulate = InputCommandType.TOOL_PRIMARY;
                    break;
            }
        }

        public void MirrorInputs()
        {
            switch(inputToSimulate)
            {
                case InputCommandType.TOOL_LEFT: inputToSimulate = InputCommandType.TOOL_RIGHT; break;
                case InputCommandType.TOOL_RIGHT: inputToSimulate = InputCommandType.TOOL_LEFT; break;
            }
        }

        private void Start()
        {
            proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.MinDistance = interactRadius;
            proximityDetector.LocalOffset = interactOffset;
            proximityDetector.ExitThreshold = interactRadius * 0.04f;
            proximityDetector.SetTrackedObjects(HandsController.Behaviour.DominantHandBehaviour.IndexTip, HandsController.Behaviour.OffHandBehaviour.IndexTip);
            proximityDetector.OnEnter += FingertipEnter;
            proximityDetector.OnExit += FingertipExit;
        }

        private void OnDisable()
        {
            isFingertipInside = false;
            ControllerInput.SimulateInput(inputToSimulate, false);
        }

        private void FingertipEnter(Transform indexTip)
        {
            if(!isFingertipInside && State != ButtonState.Disabled)
            {
                ControllerInput.SimulateInput(inputToSimulate, true);
                Hand offHand = VRToolSwapper.NonInteractingHand ?? HandsController.Behaviour.OffHandBehaviour;
                SteamVR_Actions.default_Haptic.Execute(0, 0.2f, 300, .2f * ModConfig.ModSettings.VibrationStrength, offHand.InputSource);
                SteamVR_Actions.default_Haptic.Execute(0.1f, 0.2f, 100, .1f * ModConfig.ModSettings.VibrationStrength, offHand.InputSource);
            }
            isFingertipInside = true;
        }

        private void FingertipExit(Transform indexTip)
        {
            if (isFingertipInside && State != ButtonState.Disabled)
            {
                ControllerInput.SimulateInput(inputToSimulate, false);
                Hand offHand = VRToolSwapper.NonInteractingHand ?? HandsController.Behaviour.OffHandBehaviour;
                SteamVR_Actions.default_Haptic.Execute(0, 0.1f, 100, .05f * ModConfig.ModSettings.VibrationStrength, offHand.InputSource);
            }
            isFingertipInside = false;
        }

        private float CalculateFingerTipDistance()
        {
            Hand offHand = VRToolSwapper.NonInteractingHand ?? HandsController.Behaviour.OffHandBehaviour;
            Vector3 touchableCenter = transform.position + transform.TransformVector(proximityDetector.LocalOffset);
            Vector3 touchableClosestPoint = touchableCenter + (offHand.IndexTip.position - touchableCenter).normalized * interactRadius;
            float distanceFromCenter = Vector3.Distance(offHand.IndexTip.position, touchableCenter);
            float offHandDistance = Vector3.Distance(offHand.IndexTip.position, touchableClosestPoint);
            offHand.NotifyPointable(distanceFromCenter < interactRadius ? 0 : offHandDistance);
            return offHandDistance;
        }

        protected override bool IsButtonEnabled() => CheckEnabled == null || CheckEnabled.Invoke();

        protected override bool IsButtonActive()
        {
            if (isFingertipInside) CalculateFingerTipDistance();
            return isFingertipInside;
        }

        protected override bool IsButtonFocused()
        {
            return CalculateFingerTipDistance() < Hand.minimumPointDistance*1.5f;
        }
    }
}
