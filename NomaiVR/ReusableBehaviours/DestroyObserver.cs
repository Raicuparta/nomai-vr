using System;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class DestroyObserver : MonoBehaviour
    {
        public event Action OnDestroyed;

        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}
