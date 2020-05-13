using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public Transform hand = HandsController.RightHand;

        private void Start()
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
