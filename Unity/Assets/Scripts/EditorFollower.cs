//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;
using System.Collections;
using Valve.VR;
using UnityEngine.Serialization;

namespace Valve.VR.InteractionSystem.Sample
{
    [ExecuteInEditMode]
    public class EditorFollower : MonoBehaviour
    {
        public Transform target;
        public Transform follower;
        public bool followBySearch;
        public string OtherObjectPath;

        private void Start() {
            UpdateTarget();
        }

        private void OnTransformChildrenChanged()
        {
            UpdateTarget();
        }

        private void OnValidate() {
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            Transform searchTarget = null;
            if(followBySearch && (searchTarget = transform.Find(OtherObjectPath)))
            {
                target = searchTarget;
            }
        }

        private void Update()
        {
            if(target != null && follower != null)
            {
                follower.position = target.position;
                follower.rotation = target.rotation;
            }
        }
    }
}