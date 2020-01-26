using OWML.Common;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    class HolsterTool: MonoBehaviour
    {
        public Transform hand;
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        MeshRenderer[] _renderers;
        bool _visible;
        bool _enabled = true;

        void Start() {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

            transform.localScale = Vector3.one * scale;
            transform.parent = Common.PlayerBody.transform;
            transform.localPosition = position;
            transform.localRotation = Quaternion.identity;
            transform.Rotate(angle);
        }

        void Equip() {
            FindObjectOfType<ToolModeSwapper>().EquipToolMode(mode);

            if (mode == ToolMode.Translator) {
                GameObject.FindObjectOfType<NomaiTranslatorProp>().SetValue("_currentTextID", 1);
            }
        }

        void Unequip() {
            Common.ToolSwapper.UnequipTool();
        }

        void SetVisible(bool visible) {
            foreach (var renderer in _renderers) {
                renderer.enabled = visible;
            }
            _visible = visible;
        }

        void Update() {
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
            if (ControllerInput.IsGripping && Vector3.Distance(transform.position, hand.position) < 0.2f) {
                if (Common.ToolSwapper.IsInToolMode(ToolMode.None)) {
                    Equip();
                } else if(Common.ToolSwapper.IsInToolMode(mode)) {
                    ControllerInput.ResetRB();
                    Unequip();
                }
                ControllerInput.IsGripping = false;
            }
            if (!_visible && !Common.ToolSwapper.IsInToolMode(mode)) {
                SetVisible(true);
            }
            if (_visible && Common.ToolSwapper.IsInToolMode(mode)) {
                SetVisible(false);
            }
        }
    }
}
