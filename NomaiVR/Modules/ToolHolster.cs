using OWML.Common;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    class ToolHolster: MonoBehaviour
    {
        public Transform hand;
        bool _grip = false;

        void Start() {
            SteamVR_Actions.default_RB.onChange += onRBChange;
        }

        void onRBChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            _grip = newState;

            if (!_grip) {
                Unequip();
            }
        }

        void Equip() {
            gameObject.SetActive(false);
            FindObjectOfType<ToolModeSwapper>().EquipToolMode(ToolMode.SignalScope);
        }

        void Unequip() {
            gameObject.SetActive(true);
            FindObjectOfType<ToolModeSwapper>().UnequipTool();
        }

        void Update() {
            transform.position = Common.MainCamera.transform.parent.TransformPoint(Common.MainCamera.transform.localPosition + new Vector3(0.2f, -0.7f, 0.1f));

            if (_grip && Vector3.Distance(transform.position, hand.position) < 0.3f) {
                Equip();
            }
        }
    }
}
