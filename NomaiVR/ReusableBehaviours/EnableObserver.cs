using System;
using UnityEngine;

namespace NomaiVR
{
    internal class EnableObserver : MonoBehaviour
    {
        public event Action OnActivate;
        public event Action OnDeactivate;

        internal void OnEnable()
        {
            OnActivate?.Invoke();
        }

        internal void OnDisable()
        {
            OnDeactivate?.Invoke();
        }
    }
}
