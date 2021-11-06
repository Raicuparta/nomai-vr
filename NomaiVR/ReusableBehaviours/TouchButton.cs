using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.Tools;
using System;
using UnityEngine;
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
                case "Up": inputToSimulate = InputCommandType.TOOL_UP; break;
                case "Down": inputToSimulate = InputCommandType.TOOL_DOWN; break;
                case "Left": inputToSimulate = InputCommandType.TOOL_LEFT; break;
                case "Right": inputToSimulate = InputCommandType.TOOL_RIGHT; break;
                default: inputToSimulate = InputCommandType.TOOL_PRIMARY; break;
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
            isFingertipInside = true;
            ControllerInput.SimulateInput(inputToSimulate, true);
        }

        private void FingertipExit(Transform indexTip)
        {
            isFingertipInside = false;
            ControllerInput.SimulateInput(inputToSimulate, false);
        }

        private float CalculateFingerTipDistance()
        {
            Hand offHand = VRToolSwapper.NonInteractingHand ?? HandsController.Behaviour.OffHandBehaviour;
            float offHandDistance = Vector3.Distance(offHand.IndexTip.position, transform.position + transform.TransformVector(proximityDetector.LocalOffset));
            offHand.NotifyPointable(offHandDistance);
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
            return CalculateFingerTipDistance() < Hand.k_minimumPointDistance;
        }
    }
}
