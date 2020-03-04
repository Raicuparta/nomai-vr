using UnityEngine;

namespace NomaiVR {
    class FlashlightGesture: MonoBehaviour {
        void Awake () {
            var detector = Locator.GetPlayerCamera().gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
            detector.localOffset = Vector3.right * 0.15f;
            detector.onEnter += FlashlightPress;
            detector.onExit += FlashlightRelease;
        }

        void FlashlightPress () {
            ControllerInput.SimulateInput(XboxButton.RightStickClick, 1);
        }
        void FlashlightRelease () {
            ControllerInput.SimulateInput(XboxButton.RightStickClick, 0);
        }
    }
}
