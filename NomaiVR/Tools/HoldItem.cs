using UnityEngine;

namespace NomaiVR {
    class HoldItem: MonoBehaviour {
        ItemTool _itemTool;

        void Awake () {
            _itemTool = FindObjectOfType<ItemTool>();
            _itemTool.transform.localScale = 1.8f * Vector3.one;
            Hands.HoldObject(_itemTool.transform.Find("ItemSocket"), Hands.RightHand);
            Hands.HoldObject(_itemTool.transform.Find("ScrollSocket"), Hands.RightHand, new Vector3(-0.06f, -0.06f, -0.03f), Quaternion.Euler(354f, 104f, 194f));
            Hands.HoldObject(_itemTool.transform.Find("SharedStoneSocket"), Hands.RightHand, new Vector3(-0.13f, -0.03f, 0f));
            Hands.HoldObject(_itemTool.transform.Find("WarpCoreSocket"), Hands.RightHand, new Vector3(-0.1f, -0.01f, 0.03f), Quaternion.Euler(335f, 34f, 64f));
            Hands.HoldObject(_itemTool.transform.Find("VesselCoreSocket"), Hands.RightHand, new Vector3(-0.07f, -0.03f, -0.01f), Quaternion.Euler(31.1f, 70.4f, 26f));
            Hands.HoldObject(_itemTool.transform.Find("LanternSocket"), Hands.RightHand);
        }

        void SetActive (bool active) {
            var heldItem = _itemTool.GetHeldItem();
            if (!heldItem) {
                return;
            }
            heldItem.gameObject.SetActive(active);
        }

        bool IsActive () {
            var heldItem = _itemTool.GetHeldItem();
            if (!heldItem) {
                return false;
            }
            return heldItem.gameObject.activeSelf;
        }

        void Update () {
            if (IsActive() && Common.IsUsingAnyTool()) {
                SetActive(false);
            } else if (!IsActive() && !Common.IsUsingAnyTool()) {
                SetActive(true);
            }
        }
    }
}
