using System;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    internal class ConditionalRenderer : MonoBehaviour, IActiveObserver
    {
        public Func<bool> GETShouldRender;
        private bool shouldRender;
        private Vector3 scale;
        private Renderer renderer;

        public event Action OnActivate;
        public event Action OnDeactivate;

        public bool IsActive => shouldRender;

        internal void Start()
        {
            scale = transform.localScale;
            renderer = GetComponent<Renderer>();
            SetShow(false);
        }

        internal void OnDisable()
        {
            shouldRender = false;
            OnDeactivate?.Invoke();
        }

        protected virtual void SetShow(bool show)
        {
            shouldRender = show;

            if (renderer != null)
                renderer.enabled = show;
            else
                transform.localScale = show ? scale : Vector3.zero;

            if (show)
                OnActivate?.Invoke();
            else
                OnDeactivate?.Invoke();
        }

        internal void Update()
        {
            var shouldRender = GETShouldRender.Invoke();
            if (!this.shouldRender && shouldRender)
            {
                SetShow(true);
            }
            if (this.shouldRender && !shouldRender)
            {
                SetShow(false);
            }
        }
    }
}
