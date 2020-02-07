using System;
using UnityEngine;

namespace NomaiVR {
    class Grabbable: MonoBehaviour {
        public ProximityDetector detector { get; private set; }
        public Action onGrab;
        public Action onRelease;
        bool _isGrabbing;

        void Awake () {
            detector = gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
        }

        void Grab () {
            _isGrabbing = true;
            onGrab?.Invoke();
        }

        void Release () {
            _isGrabbing = false;
            onRelease?.Invoke();
        }

        void Update () {
            if (!OWInput.IsInputMode(InputMode.Character)) {
                if (_isGrabbing) {
                    Release();
                }
                return;
            }
            if (!_isGrabbing && ControllerInput.IsGripping && detector.isInside) {
                Grab();
            }
            if (_isGrabbing && !ControllerInput.IsGripping && !Common.ToolSwapper.IsInToolMode(ToolMode.None)) {
                Release();
            }
        }
    }
}
