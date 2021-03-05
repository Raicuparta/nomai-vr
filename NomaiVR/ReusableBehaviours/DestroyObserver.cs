using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
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
