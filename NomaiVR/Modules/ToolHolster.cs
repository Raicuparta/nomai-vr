using OWML.Common;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    class ToolHolster: MonoBehaviour
    {
        public Transform hand;
        public ToolMode mode;
        public float offset;
        public float scale;
        MeshRenderer[] _renderers;

        void Start() {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

            transform.localScale = Vector3.one * scale;
            transform.parent = Common.PlayerBody.transform;
            transform.localPosition = new Vector3(offset, 0.2f, 0.2f);
            transform.localRotation = Quaternion.identity;
            transform.Rotate(Vector3.right * 90);
        }

        void Equip() {
            SetRenderersEnabled(false);
            FindObjectOfType<ToolModeSwapper>().EquipToolMode(mode);
        }

        void Unequip() {
            SetRenderersEnabled(true);
            Common.ToolSwapper.UnequipTool();
        }

        void SetRenderersEnabled(bool enabled) {
            foreach (var renderer in _renderers) {
                renderer.enabled = enabled;
            }
        }

        void Update() {
            if (ControllerInput.IsGripping && Vector3.Distance(transform.position, hand.position) < 0.2f) {
                if (Common.ToolSwapper.IsInToolMode(ToolMode.None)) {
                    Equip();
                } else if(Common.ToolSwapper.IsInToolMode(mode)) {
                    Unequip();
                }
                ControllerInput.IsGripping = false;
            }
        }
    }
}
