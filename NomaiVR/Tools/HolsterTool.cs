using System;
using NomaiVR.Hands;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.Tools
{
    internal class HolsterTool : MonoBehaviour
    {
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        private MeshRenderer[] renderers;
        private bool visible = true;
        private bool showInDream = false;
        private int equippedIndex = -1;
        private Transform hand;
        public Action ONUnequip;
        private const float minHandDistance = 0.3f;
        private Transform cachedTransform;

        public ProximityDetector Detector { get; private set; }

        internal void Start()
        {
            Detector = gameObject.AddComponent<ProximityDetector>();
            Detector.SetTrackedObjects(HandsController.Behaviour.RightHand, HandsController.Behaviour.LeftHand);
            Detector.MinDistance = 0.2f;

            Detector.OnEnter += (hand) => { hand.GetComponent<Hand>().NotifyReachable(true); };
            Detector.OnExit += (hand) => { hand.GetComponent<Hand>().NotifyReachable(false); };

            renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            transform.localScale = Vector3.one * scale;
            cachedTransform = transform;

            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.AddOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
            GlobalMessenger.AddListener("SuitUp", Unequip);
            GlobalMessenger.AddListener("RemoveSuit", Unequip);
        }

        internal void OnDestroy()
        {
            GlobalMessenger.RemoveListener("SuitUp", Unequip);
            GlobalMessenger.RemoveListener("RemoveSuit", Unequip);
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.RightHand);
            SteamVR_Actions.default_Grip.RemoveOnChangeListener(OnGripUpdated, SteamVR_Input_Sources.LeftHand);
        }

        private void Equip(int handIndex)
        {
            hand = Detector.GetTrackedObject(handIndex);
            equippedIndex = handIndex;
            VRToolSwapper.Equip(mode, hand.GetComponent<Hand>());
        }

        private void Unequip()
        {
            hand = null;
            equippedIndex = -1;
            ONUnequip?.Invoke();
            VRToolSwapper.Unequip();
        }

        private void SetVisible(bool visible)
        {
            foreach (var renderer in renderers)
            {
                renderer.enabled = visible;
            }
            Detector.enabled = visible;
            this.visible = visible;
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
            if (fromAction.GetState(fromSource) && hand == null && !IsEquipped() && Detector.IsInside(handIndex) && visible && ToolHelper.IsUsingNoTools())
                Equip(handIndex);
            else if (!fromAction.GetState(fromSource) && equippedIndex == handIndex && IsEquipped())
                Unequip();
        }

        private bool IsHandNear(Transform hand) => (hand.position - cachedTransform.position).sqrMagnitude < minHandDistance;

        private void UpdateDreamVisibility(bool isInDream)
        {
            if (!isInDream) showInDream = false;
            else showInDream |= (mode == ToolMode.Translator && VRToolSwapper.NomaiTextFocused);
        }

        private void UpdateVisibility()
        {
            var isCharacterMode = OWInput.IsInputMode(InputMode.Character);
            var isHoldingVisionTorch = Locator.GetToolModeSwapper()?.GetItemCarryTool()?.GetHeldItemType() == ItemType.VisionTorch;
            var isHandClose = !ModSettings.AutoHideToolbelt || IsHandNear(HandsController.Behaviour.RightHand) || IsHandNear(HandsController.Behaviour.LeftHand);
            var isInDream = Locator.GetDreamWorldController() != null && Locator.GetDreamWorldController().IsInDream();

            UpdateDreamVisibility(isInDream);

            var shouldBeVisible = !ToolHelper.IsUsingAnyTool() && isCharacterMode && (!isInDream || showInDream) && !isHoldingVisionTorch && isHandClose;

            if (!visible && shouldBeVisible)
            {
                SetVisible(true);
            }
            if (visible && !shouldBeVisible)
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
            cachedTransform.position = Locator.GetPlayerCamera().transform.position + player.TransformVector(position);
            cachedTransform.rotation = player.rotation;
            cachedTransform.Rotate(angle);
        }
    }
}
