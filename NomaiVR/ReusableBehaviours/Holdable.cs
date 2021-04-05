using OWML.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public Transform hand = HandsController.Behaviour.RightHand;
        public bool CanFlipX { get; set; } = true;
        public Action<bool> onFlipped;

        private Transform _holdableTransform;
        private Vector3 _positionOffset;

        internal void Start()
        {
            _holdableTransform = new GameObject().transform;
            _holdableTransform.parent = hand;
            _holdableTransform.localPosition = (_positionOffset = transform.localPosition);
            _holdableTransform.localRotation = transform.localRotation;
            transform.parent = _holdableTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            var tool = gameObject.GetComponent<PlayerTool>();
            if (tool)
            {
                tool._stowTransform = null;
                tool._holdTransform = null;
            }
        }

        //FIXME: Do it better
        internal void Update()
        {
            if(VRToolSwapper.InteractingHand?.transform != hand)
            {
                hand = VRToolSwapper.InteractingHand?.transform;
                if (hand == null) hand = HandsController.Behaviour.RightHand;
                _holdableTransform.SetParent(hand, false);

                var isRight = hand == HandsController.Behaviour.RightHand;
                if (isRight)
                {
                    _holdableTransform.localScale = new Vector3(1, 1, 1);
                    _holdableTransform.localPosition = _positionOffset;
                }
                else
                {
                    if (CanFlipX)
                        _holdableTransform.localScale = new Vector3(-1, 1, 1);
                    _holdableTransform.localPosition = new Vector3(-_positionOffset.x, _positionOffset.y, _positionOffset.z);
                }

                if (CanFlipX)
                {
                    RestoreCanvases(isRight);
                    onFlipped?.Invoke(isRight);
                }
            }
        }

        internal void RestoreCanvases(bool isRight)
        {
            //Assures canvases are always scaled with x > 0
            Array.ForEach(transform.GetComponentsInChildren<Canvas>(true), canvas =>
            {
                Transform canvasTransform = canvas.transform;
                Vector3 canvasScale = canvasTransform.localScale;
                float tagetScale = Mathf.Abs(canvasScale.x);
                if (!isRight) tagetScale *= -1;
                canvasTransform.localScale = new Vector3(tagetScale, canvasScale.y, canvasScale.z);
            });
        }
    }
}
