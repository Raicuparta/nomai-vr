﻿using OWML.Utils;
using UnityEngine;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public Transform hand = HandsController.Behaviour.RightHand;

        internal void Start()
        {
            var objectParent = new GameObject().transform;
            objectParent.parent = hand;
            objectParent.localPosition = transform.localPosition;
            objectParent.localRotation = transform.localRotation;
            transform.parent = objectParent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            var tool = gameObject.GetComponent<PlayerTool>();
            if (tool)
            {
                tool.SetValue("_stowTransform", null);
                tool.SetValue("_holdTransform", null);
            }
        }
    }
}
