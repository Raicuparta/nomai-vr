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
            detector.onEnter += OnEnter;
            detector.onExit += OnExit;
        }

        void OnEnter () {
            if (_grabbing && !ControllerInput.IsGripping) {
                onRelease?.Invoke();
                _grabbing = false;
            }
        }

        void OnExit () {
            if (!_grabbing && ControllerInput.IsGripping) {
                onGrab?.Invoke();
                _grabbing = true;
            }
        }
    }
}
