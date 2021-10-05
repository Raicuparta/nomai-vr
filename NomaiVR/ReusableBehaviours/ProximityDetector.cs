using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ProximityDetector : MonoBehaviour
    {
        public event Action OnStay;
        public event Action<Transform> OnEnter;
        public event Action<Transform> OnExit;
        public float MinDistance { get; set; } = 0.15f;
        public float ExitThreshold { get; set; } = 0.01f;
        public Vector3 LocalOffset { get; set; }

        public Transform Other 
        { 
            get
            {
                return _trackedObjects != null ? GetTrackedObject() : null;
            }
            set
            {
                if (_trackedObjects != null)
                {
                    _trackedObjects[0] = value;
                    _isInside[0] = false;
                }
                else
                {
                    _trackedObjects = new[] { value };
                    _isInside = new bool[1];
                }
            }
        }

        private bool[] _isInside;
        private Transform[] _trackedObjects;

        public void SetTrackedObjects(params Transform[] others)
        {
            if (others != null && others.Length > 0)
            {
                _trackedObjects = others;
                _isInside = new bool[others.Length];
            }
        }

        public bool IsInside(int index = 0)
        {
            if (index >= _isInside.Length)
                return false;
            return _isInside[index];
        }

        public Transform GetTrackedObject(int index = 0)
        {
            if (index >= _trackedObjects.Length)
                return null;
            return _trackedObjects[index];
        }

        internal void Update()
        {
            bool isStaying = false;
            for(int i = 0; i < _trackedObjects.Length; i++)
            {
                var other = _trackedObjects[i];

                if (!other.gameObject.activeSelf)
                    continue;

                var offset = transform.TransformVector(LocalOffset);
                var distance = Vector3.Distance(transform.position + offset, other.position);

                if (!_isInside[i] && distance <= MinDistance)
                {
                    OnEnter?.Invoke(other);
                    _isInside[i] = true;
                }

                if (_isInside[i] && distance > MinDistance + ExitThreshold)
                {
                    OnExit?.Invoke(other);
                    _isInside[i] = false;
                }
                isStaying |= _isInside[i];
            }
            if (isStaying) OnStay?.Invoke();
        }
    }
}
