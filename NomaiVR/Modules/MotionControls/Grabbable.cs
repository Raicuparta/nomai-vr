using System;
using UnityEngine;

namespace NomaiVR {
    class Grabbable: MonoBehaviour {
        public ProximityDetector detector { get; private set; }
        public Action onGrab;
        public Action onRelease;
        bool _grabbing;
        bool _grabbed;

        void Awake () {
            detector = gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
        }

        void Update () {
            if (!detector.isInside || !OWInput.IsInputMode(InputMode.Character)) {
                return;
            }
            if (ControllerInput.IsGripping) {
                if (!_grabbed && !_grabbing) {
                    _grabbing = true;
                    onGrab?.Invoke();
                } else if (_grabbing && _grabbed) {
                    _grabbing = false;
                    onRelease?.Invoke();
                }
            } else if ((!_grabbed && _grabbing) || (_grabbed && !_grabbing)) {
                _grabbed = !_grabbed && _grabbing;
            }
        }
    }
}
