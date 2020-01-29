using System;
using UnityEngine;

namespace NomaiVR {
    class Grabbable: MonoBehaviour {
        public ProximityDetector detector { get; private set; }
        public Action onGrab;
        public Action onRelease;
        bool _grabbing;

        void Awake () {
            detector = gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
        }

        void Update () {
            if (!detector.isInside) {
                return;
            }
            if (!_grabbing && ControllerInput.IsGripping) {
                onGrab.Invoke();
            } else if (_grabbing && !ControllerInput.IsGripping) {
                onRelease.Invoke();
            }
        }
    }
}
