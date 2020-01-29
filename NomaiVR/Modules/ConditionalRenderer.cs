using System;
using UnityEngine;

namespace NomaiVR {
    class ConditionalRenderer: MonoBehaviour {
        public Func<bool> getShouldRender;
        bool _shouldRender;
        Renderer[] renderers;

        void Start () {
            renderers = GetComponentsInChildren<Renderer>();
            SetShow(false);
        }

        void SetShow (bool show) {
            _shouldRender = show;
            foreach (var renderer in renderers) {
                renderer.enabled = show;
            }
        }

        void Update () {
            var shouldRender = getShouldRender.Invoke();
            if (!_shouldRender && shouldRender) {
                SetShow(true);
            }
            if (_shouldRender && !shouldRender) {
                SetShow(false);
            }
        }
    }
}
