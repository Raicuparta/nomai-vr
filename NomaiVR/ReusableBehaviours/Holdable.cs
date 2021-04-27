using OWML.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public bool IsOffhand { get; set; } = false;
        private Transform _hand = HandsController.Behaviour.DominantHand;
        public bool CanFlipX { get; set; } = true;
        public Action<bool> onFlipped;

        private Transform _holdableTransform;
        private Vector3 _positionOffset;

        internal void Start()
        {
            _holdableTransform = new GameObject().transform;
            _holdableTransform.parent = _hand;
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

            VRToolSwapper.InteractiveHandChanged += OnInteractingHandChanged;
        }

        internal void OnDestroy()
        {
            VRToolSwapper.InteractiveHandChanged -= OnInteractingHandChanged;
        }

        internal void OnInteractingHandChanged()
        {
            if(VRToolSwapper.InteractingHand?.transform != _hand)
            {
                _hand = IsOffhand ? VRToolSwapper.NonInteractingHand?.transform : VRToolSwapper.InteractingHand?.transform;
                if (_hand == null) _hand = IsOffhand ? HandsController.Behaviour.OffHand : HandsController.Behaviour.DominantHand;
                _holdableTransform.SetParent(_hand, false);

                var isRight = _hand == HandsController.Behaviour.RightHand;
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
