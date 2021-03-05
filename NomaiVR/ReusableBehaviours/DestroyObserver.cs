using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class DestroyObserver : MonoBehaviour
    {
        public event Action OnObjectDestroyed;

        private void OnDestroy()
        {
            OnObjectDestroyed?.Invoke();
        }
    }
}
