using OWML.ModHelper.Events;
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
        public Action onEquip;
        public Action onUnequip;

        public ProximityDetector detector { get; private set; }

        private void Start()
        {
            detector = gameObject.AddComponent<ProximityDetector>();
            detector.other = HandsController.Behaviour.RightHand;
            detector.minDistance = 0.2f;
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            transform.localScale = Vector3.one * scale;

            GlobalMessenger.AddListener("SuitUp", Unequip);
            GlobalMessenger.AddListener("RemoveSuit", Unequip);
        }

        private void Equip()
        {
            onEquip?.Invoke();
            ToolHelper.Swapper.EquipToolMode(mode);

            if (mode == ToolMode.Translator)
            {
                GameObject.FindObjectOfType<NomaiTranslatorProp>().SetValue("_currentTextID", 1);
            }
        }

        private void Unequip()
        {
            onUnequip?.Invoke();
            ToolHelper.Swapper.UnequipTool();
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
            if (ControllerInput.Behaviour.IsGripping && !IsEquipped() && detector.isInside && _visible)
            {
                Equip();
            }
            if (!ControllerInput.Behaviour.IsGripping && IsEquipped() && !ToolHelper.IsInProbeTrigger())
            {
                Unequip();
            }
        }

        private void UpdateVisibility()
        {
            var isCharacterMode = OWInput.IsInputMode(InputMode.Character);
            var shouldBeVisible = !ToolHelper.IsUsingAnyTool() && isCharacterMode;

            if (!_visible && shouldBeVisible)
            {
                SetVisible(true);
            }
            if (_visible && !shouldBeVisible)
            {
                SetVisible(false);
            }
        }

        private void Update()
        {
            UpdateGrab();
            UpdateVisibility();
        }

        private void LateUpdate()
        {
            if (_visible)
            {
                var player = Locator.GetPlayerTransform();
                transform.position = Camera.main.transform.position + player.TransformVector(position);
                transform.rotation = player.rotation;
                transform.Rotate(angle);
            }
        }
    }
}
