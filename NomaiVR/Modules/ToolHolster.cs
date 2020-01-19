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
            transform.parent = Common.MainCamera.transform.parent;
            transform.localRotation = Quaternion.identity;
            transform.Rotate(Vector3.right * 90);
        }

        void Equip() {
            SetRenderersEnabled(false);
            FindObjectOfType<ToolModeSwapper>().EquipToolMode(mode);
        }

        void Unequip() {
            SetRenderersEnabled(true);
        }

        void SetRenderersEnabled(bool enabled) {
            foreach (var renderer in _renderers) {
                renderer.enabled = enabled;
            }
        }

        void Update() {
            transform.position = Common.MainCamera.transform.parent.TransformPoint(Common.MainCamera.transform.localPosition + new Vector3(offset, -0.7f, 0.1f));

            if (ControllerInput.IsGripping && Common.ToolSwapper.GetToolMode() == ToolMode.None && Vector3.Distance(transform.position, hand.position) < 0.3f) {
                Equip();
            }
            if (!ControllerInput.IsGripping) {
                Unequip();
            }
        }
    }
}
