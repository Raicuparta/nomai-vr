using UnityEngine;

namespace NomaiVR {
    class ToolModeInteraction: MonoBehaviour {
        void Awake () {
            var proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.onEnter = OnDetectorEnter;
            proximityDetector.onExit = OnDetectorExit;
            proximityDetector.other = Hands.LeftHand;
            proximityDetector.exitThreshold = 0.1f;
        }

        void OnDetectorEnter () {
            ControllerInput.SimulateInput(XboxAxis.dPadX, 1);
        }

        void OnDetectorExit () {
            ControllerInput.SimulateInput(XboxAxis.dPadX, 0);
        }
    }
}
