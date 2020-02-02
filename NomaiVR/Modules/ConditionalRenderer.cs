using System;
using UnityEngine;

namespace NomaiVR {
    class ConditionalRenderer: MonoBehaviour {
        public Func<bool> getShouldRender;
        bool _shouldRender;
        Renderer[] _renderers;
        Canvas[] _canvases;

        void Start () {
            _renderers = GetComponentsInChildren<Renderer>();
            _canvases = GetComponentsInChildren<Canvas>();
            SetShow(false);
        }

        void SetShow (bool show) {
            _shouldRender = show;
            foreach (var renderer in _renderers) {
                renderer.enabled = show;
            }
            foreach (var canvas in _canvases) {
                canvas.enabled = show;
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
