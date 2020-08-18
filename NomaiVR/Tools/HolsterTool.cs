using System;
using UnityEngine;

namespace NomaiVR
{
    internal class HolsterTool : MonoBehaviour
    {
        public Transform hand;
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        private MeshRenderer[] _renderers;
        private bool _visible = true;
        public Action onUnequip;
        private const float _minHandDistance = 0.3f;

        public ProximityDetector Detector { get; private set; }

        internal void Start()
        {
            Detector = gameObject.AddComponent<ProximityDetector>();
            Detector.other = HandsController.Behaviour.RightHand;
            Detector.minDistance = 0.2f;
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            transform.localScale = Vector3.one * scale;

            GlobalMessenger.AddListener("SuitUp", Unequip);
            GlobalMessenger.AddListener("RemoveSuit", Unequip);
        }

        private void Equip()
        {
            VRToolSwapper.Equip(mode);
        }

        private void Unequip()
        {
            onUnequip?.Invoke();
            VRToolSwapper.Unequip();
        }

        private void SetVisible(bool visible)
        {
            foreach (var renderer in _renderers)
            {
                renderer.enabled = visible;
            }
            _visible = visible;
        }

        private bool IsEquipped()
        {
            return ToolHelper.Swapper.IsInToolMode(mode, ToolGroup.Suit);
        }

        private void UpdateGrab()
        {
            if (!OWInput.IsInputMode(InputMode.Character))
            {
                if (IsEquipped())
                {
                    Unequip();
                }
                return;
            }
            if (ControllerInput.Behaviour.IsGripping && !IsEquipped() && Detector.isInside && _visible)
            {
                Equip();
            }
            if (!ControllerInput.Behaviour.IsGripping && IsEquipped())
            {
                Unequip();
            }
        }

        private void UpdateVisibility()
        {
            var isCharacterMode = OWInput.IsInputMode(InputMode.Character);
            var hand = HandsController.Behaviour.RightHand;

            var isHandClose = !ModSettings.AutoHideToolbelt || (hand.position - transform.position).sqrMagnitude < _minHandDistance;
            var shouldBeVisible = !ToolHelper.IsUsingAnyTool() && isCharacterMode && isHandClose;

            if (!_visible && shouldBeVisible)
            {
                SetVisible(true);
            }
            if (_visible && !shouldBeVisible)
            {
                SetVisible(false);
            }
        }

        internal void LateUpdate()
        {
            UpdateGrab();
            UpdateVisibility();
            var player = Locator.GetPlayerTransform();
            position.y = ModSettings.ToolbeltHeight;
            transform.position = Locator.GetPlayerCamera().transform.position + player.TransformVector(position);
            transform.rotation = player.rotation;
            transform.Rotate(angle);
        }
    }
}
