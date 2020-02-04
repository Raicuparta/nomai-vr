using System;
using UnityEngine;

namespace NomaiVR {
    class ConditionalRenderer: MonoBehaviour {
        public Func<bool> getShouldRender;
        bool _shouldRender;
        Vector3 _scale;

        void Start () {
            _scale = transform.localScale;
            SetShow(false);
        }

        void SetShow (bool show) {
            _shouldRender = show;
            transform.localScale = show ? _scale : Vector3.zero;
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
