using System;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class HolsterTool : MonoBehaviour
    {
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        private MeshRenderer[] _renderers;
        private bool _visible = true;
        private int _equippedIndex = -1;
        private Transform _hand;
        public Action onUnequip;
        private const float _minHandDistance = 0.3f;

        public ProximityDetector Detector { get; private set; }

        internal void Start()
        {
            Detector = gameObject.AddComponent<ProximityDetector>();
            Detector.SetTrackedObjects(HandsController.Behaviour.RightHand, HandsController.Behaviour.LeftHand);
            Detector.MinDistance = 0.2f;
            
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            transform.localScale = Vector3.one * scale;

            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
            GlobalMessenger.AddListener("SuitUp", Unequip);
            GlobalMessenger.AddListener("RemoveSuit", Unequip);
        }

        internal void OnDestroy()
        {
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
        }

        private void Equip(int handIndex)
        {
            _hand = Detector.GetTrackedObject(handIndex);
            _equippedIndex = handIndex;
            VRToolSwapper.Equip(mode, _hand.GetComponent<Hand>());
        }

        private void Unequip()
        {
            _hand = null;
            _equippedIndex = -1;
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
            if (!OWInput.IsInputMode(InputMode.Character) && IsEquipped())
            {
                Unequip();
            }
        }

        private void OnGripUpdated(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
        {
            var handIndex = fromSource == SteamVR_Input_Sources.RightHand ? 0 : 1;
            if (fromAction.GetState(fromSource) && _hand == null && !IsEquipped() && Detector.IsInside(handIndex) && _visible)
                Equip(handIndex);
            else if (!fromAction.GetState(fromSource) && _equippedIndex == handIndex && IsEquipped())
                Unequip();
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
