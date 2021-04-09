using System;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class HolsterTool : MonoBehaviour
    {
        private Transform _hand;
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        private MeshRenderer[] _renderers;
        private bool _visible = true;
        public Action onUnequip;
        private const float _minHandDistance = 0.3f;

        public ProximityDetector[] Detectors { get; private set; }

        internal void Start()
        {
            Detectors = new ProximityDetector[2];
            Detectors[0] = gameObject.AddComponent<ProximityDetector>();
            Detectors[0].other = HandsController.Behaviour.RightHand;
            Detectors[0].minDistance = 0.2f;
            Detectors[1] = gameObject.AddComponent<ProximityDetector>();
            Detectors[1].other = HandsController.Behaviour.LeftHand;
            Detectors[1].minDistance = 0.2f;
            
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            transform.localScale = Vector3.one * scale;

            SteamVR_Actions.default_Grip.onChange += OnGripChanged;
            GlobalMessenger.AddListener("SuitUp", Unequip);
            GlobalMessenger.AddListener("RemoveSuit", Unequip);
        }

        internal void OnDestroy()
        {
            SteamVR_Actions.default_Grip.onChange -= OnGripChanged;
        }

        private void Equip()
        {
            VRToolSwapper.Equip(mode, _hand.GetComponent<Hand>());
        }

        private void Unequip()
        {
            _hand = null;
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
        }

        private void OnGripChanged(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            var detector = Detectors[fromAction.activeDevice == SteamVR_Input_Sources.RightHand ? 0 : 1];
            if (newState && _hand == null && !IsEquipped() && detector.isInside && _visible)
            {
                _hand = detector.other;
                Equip();
            }
            if (!newState && detector.other == _hand && IsEquipped())
            {
                Unequip();
            }
        }

        private void UpdateVisibility()
        {
            var isCharacterMode = OWInput.IsInputMode(InputMode.Character);
            Func<Transform,bool> handNear = (hand) => (hand.position - transform.position).sqrMagnitude < _minHandDistance;

            var isHandClose = !ModSettings.AutoHideToolbelt || handNear(HandsController.Behaviour.RightHand) || handNear(HandsController.Behaviour.LeftHand);
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
