using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ConditionalRenderer : MonoBehaviour, IActiveObserver
    {
        public Func<bool> getShouldRender;
        private bool _shouldRender;
        private Vector3 _scale;
        private Renderer _renderer;

        public event Action OnActivate;
        public event Action OnDeactivate;

        public bool IsActive => _shouldRender;

        internal void Start()
        {
            _scale = transform.localScale;
            _renderer = GetComponent<Renderer>();
            SetShow(false);
        }

        internal void OnDisable()
        {
            _shouldRender = false;
            OnDeactivate?.Invoke();
        }

        protected virtual void SetShow(bool show)
        {
            _shouldRender = show;

            if (_renderer != null)
                _renderer.enabled = show;
            else
                transform.localScale = show ? _scale : Vector3.zero;

            if (show)
                OnActivate?.Invoke();
            else
                OnDeactivate?.Invoke();
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
