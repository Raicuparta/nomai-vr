using UnityEngine;

namespace NomaiVR {
    class HoldItem: MonoBehaviour {
        void Awake () {
            var itemCarryTool = GameObject.FindObjectOfType<ItemTool>().transform;
            itemCarryTool.localScale = 1.8f * Vector3.one;
            Hands.HoldObject(itemCarryTool.Find("ScrollSocket"), Hands.RightHand, new Vector3(-0.06f, -0.06f, -0.06f), Quaternion.Euler(4.5f, 286f, 320f));
            Hands.HoldObject(itemCarryTool.Find("ItemSocket"), Hands.RightHand);
            Hands.HoldObject(itemCarryTool.Find("SharedStoneSocket"), Hands.RightHand, new Vector3(-0.13f, -0.03f, 0f));
            Hands.HoldObject(itemCarryTool.Find("WarpCoreSocket"), Hands.RightHand);
            Hands.HoldObject(itemCarryTool.Find("VesselCoreSocket"), Hands.RightHand);
            Hands.HoldObject(itemCarryTool.Find("LanternSocket"), Hands.RightHand);
        }
    }
}
