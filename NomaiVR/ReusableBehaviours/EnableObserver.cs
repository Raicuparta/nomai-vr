using System;
using UnityEngine;

namespace NomaiVR
{
    internal class EnableObserver : MonoBehaviour, IActiveObserver
    {
        public bool IsActive { get; private set; }
        public event Action OnActivate;
        public event Action OnDeactivate;

        internal void OnEnable()
        {
            OnActivate?.Invoke();
            IsActive = true;
        }

        internal void OnDisable()
        {
            OnDeactivate?.Invoke();
            IsActive = false;
        }
    }
}
