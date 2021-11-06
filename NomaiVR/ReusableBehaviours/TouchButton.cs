using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.Tools;
using UnityEngine;
using static InputConsts;

namespace NomaiVR.ReusableBehaviours
{
    public class TouchButton : GlowyButton
    {
        private float interactRadius = 0.1f;
        private Vector3 interactOffset = Vector3.zero;
        private ProximityDetector proximityDetector;
        private bool isFingertipInside = false;
        private InputCommandType inputToSimulate;

        protected override void Initialize()
        {
            Logs.WriteError($"Touch button for {name}");
            var localSphereCollider = GetComponent<SphereCollider>();
            if (localSphereCollider != null)
            {
                interactRadius = localSphereCollider.radius;
                interactOffset = localSphereCollider.center;
            }

            switch(name)
            {
                case "Up": inputToSimulate = InputCommandType.TOOL_UP; break;
                case "Down": inputToSimulate = InputCommandType.TOOL_DOWN; break;
                case "Left": inputToSimulate = InputCommandType.TOOL_LEFT; break;
                case "Right": inputToSimulate = InputCommandType.TOOL_RIGHT; break;
                default: inputToSimulate = InputCommandType.TOOL_PRIMARY; break;
            }
        }

        private void Start()
        {
            proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.MinDistance = interactRadius;
            proximityDetector.LocalOffset = interactOffset;
            proximityDetector.ExitThreshold = interactRadius + (interactRadius * 0.04f);
            proximityDetector.SetTrackedObjects(HandsController.Behaviour.DominantHandBehaviour.IndexTip, HandsController.Behaviour.OffHandBehaviour.IndexTip);
            proximityDetector.OnEnter += FingertipEnter;
            proximityDetector.OnExit += FingertipExit;
        }

        private void OnEnable()
        {
            isFingertipInside = false;
        }

        private void FingertipEnter(Transform indexTip)
        {
            isFingertipInside = true;
            ControllerInput.SimulateInput(inputToSimulate, true);
            Invoke(nameof(DisableInput), 0.2f);
        }

        private void FingertipExit(Transform indexTip)
        {
            isFingertipInside = false;
        }

        private void DisableInput()
        {
            ControllerInput.SimulateInput(inputToSimulate, false);
        }

        protected virtual bool IsEnabled() => true;

        protected override bool IsButtonEnabled() => IsEnabled();

        protected override bool IsButtonActive() => isFingertipInside;

        protected override bool IsButtonFocused()
        {
            Hand offHand = VRToolSwapper.NonInteractingHand ?? HandsController.Behaviour.OffHandBehaviour;
            float offHandDistance = Vector3.Distance(offHand.IndexTip.position, transform.position + transform.TransformVector(proximityDetector.LocalOffset));
            offHand.NotifyPointable(offHandDistance);
            return offHandDistance < Hand.k_minimumPointDistance;
        }
    }
}
