using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ChildThresholdObserver : MonoBehaviour, IActiveObserver
    {
        public bool IsActive => _wasActive;
        public event Action OnActivate;
        public event Action OnDeactivate;
        public int childThreshold = 1;

        private bool _wasActive;
        private Transform _transform;

        internal void Start()
        {
            _transform = transform;
        }

        internal void OnTransformChildrenChanged()
        {
            if (!_wasActive && _transform.childCount >= childThreshold)
            {
                _wasActive = true;
                OnActivate?.Invoke();
            }    
            else if(_wasActive && _transform.childCount < childThreshold)
            {
                OnDeactivate?.Invoke();
                _wasActive = false;
            }
        }
    }
}
