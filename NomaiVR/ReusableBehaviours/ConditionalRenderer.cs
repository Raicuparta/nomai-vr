using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ConditionalRenderer : MonoBehaviour
    {
        public Func<bool> getShouldRender;
        private bool _shouldRender;
        private Vector3 _scale;

        internal void Start()
        {
            _scale = transform.localScale;
            SetShow(false);
        }

        private void SetShow(bool show)
        {
            _shouldRender = show;
            transform.localScale = show ? _scale : Vector3.zero;
        }

        internal void Update()
        {
            var shouldRender = getShouldRender.Invoke();
            if (!_shouldRender && shouldRender)
            {
                SetShow(true);
            }
            if (_shouldRender && !shouldRender)
            {
                SetShow(false);
            }
        }
    }
}
