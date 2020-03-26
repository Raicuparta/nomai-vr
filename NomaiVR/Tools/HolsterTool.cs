using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR {
    class HolsterTool: MonoBehaviour {
        public Transform hand;
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        MeshRenderer[] _renderers;
        bool _visible;
        bool _enabled = true;
        public Action onEquip;
        public Action onUnequip;

        public ProximityDetector detector { get; private set; }

        void Awake () {
            detector = gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
            detector.minDistance = 0.2f;
        }

        void Start () {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            transform.localScale = Vector3.one * scale;

            GlobalMessenger.AddListener("SuitUp", Unequip);
            GlobalMessenger.AddListener("RemoveSuit", Unequip);
        }

        void Equip () {
            onEquip?.Invoke();
            Locator.GetToolModeSwapper().EquipToolMode(mode);

            if (mode == ToolMode.Translator) {
                GameObject.FindObjectOfType<NomaiTranslatorProp>().SetValue("_currentTextID", 1);
            }
        }

        void Unequip () {
            onUnequip?.Invoke();
            Common.ToolSwapper.UnequipTool();
        }

        void SetVisible (bool visible) {
            foreach (var renderer in _renderers) {
                renderer.enabled = visible;
            }
            _visible = visible;
        }

        bool IsEquipped () {
            return Locator.GetToolModeSwapper().IsInToolMode(mode, ToolGroup.Suit);
        }

        void UpdateGrab () {
            if (!OWInput.IsInputMode(InputMode.Character)) {
                if (IsEquipped()) {
                    Unequip();
                }
                return;
            }
            if (ControllerInput.IsGripping && !IsEquipped() && detector.isInside && _visible) {
                Equip();
            }
            if (!ControllerInput.IsGripping && IsEquipped()) {
                Unequip();
            }
        }

        void UpdateVisibility () {
            if (_enabled && !OWInput.IsInputMode(InputMode.Character)) {
                _enabled = false;
                SetVisible(false);
            }
            if (!_enabled && OWInput.IsInputMode(InputMode.Character)) {
                _enabled = true;
            }
            if (!_enabled) {
                return;
            }
            if (!_visible && !Common.IsUsingTool()) {
                SetVisible(true);
            }
            if (_visible && Common.IsUsingTool()) {
                SetVisible(false);
            }
        }

        void Update () {
            UpdateGrab();
            UpdateVisibility();
        }

        void LateUpdate () {
            if (!_enabled) {
                return;
            }
            if (_enabled && _visible) {
                transform.position = Camera.main.transform.position + Common.PlayerBody.transform.TransformVector(position);
                transform.rotation = Common.PlayerBody.transform.rotation;
                transform.Rotate(angle);
            }
        }
    }
}
