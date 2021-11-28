using System;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    internal class ChildThresholdObserver : MonoBehaviour, IActiveObserver
    {
        public bool IsActive => wasActive;
        public event Action OnActivate;
        public event Action OnDeactivate;
        public int childThreshold = 1;

        private bool wasActive;
        private Transform selfTransform;

        internal void Start()
        {
            selfTransform = ((Component) this).transform;
        }

        internal void OnTransformChildrenChanged()
        {
            if (!wasActive && selfTransform.childCount >= childThreshold)
            {
                wasActive = true;
                OnActivate?.Invoke();
            }    
            else if(wasActive && selfTransform.childCount < childThreshold)
            {
                OnDeactivate?.Invoke();
                wasActive = false;
            }
        }
    }
}
