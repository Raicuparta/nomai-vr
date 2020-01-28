using System;
using UnityEngine;

namespace NomaiVR
{
    class ProximityDetector: MonoBehaviour {
        public float minDistance = 0.15f;
        public float exitThreshold = 0.01f;
        public Transform other;
        public Action onEnter;
        public Action onExit;
        bool _entered;

        void Update() {
            if (!other.gameObject.activeSelf) {
                return;
            }
            var distance = Vector3.Distance(transform.position, other.position);
            if (!_entered && distance <= minDistance) {
                if (onEnter != null) {
                    onEnter.Invoke();
                }
                _entered = true;
            }
            if (_entered && distance > minDistance + exitThreshold) {
                if (onExit != null) {
                    onExit.Invoke();
                }
                _entered = false;
            }
        }
    }
}
