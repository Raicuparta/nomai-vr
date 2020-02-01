using UnityEngine;

namespace NomaiVR {
    class HoldItem: MonoBehaviour {
        void Awake () {
            var itemCarryTool = GameObject.FindObjectOfType<ItemTool>().transform;
            itemCarryTool.localScale = 1.8f * Vector3.one;
            Hands.HoldObject(itemCarryTool.Find("ScrollSocket"), Hands.RightHand, new Vector3(-0.06f, -0.06f, -0.03f), Quaternion.Euler(354f, 104f, 194f));
            Hands.HoldObject(itemCarryTool.Find("ItemSocket"), Hands.RightHand);
            Hands.HoldObject(itemCarryTool.Find("SharedStoneSocket"), Hands.RightHand, new Vector3(-0.13f, -0.03f, 0f));
            Hands.HoldObject(itemCarryTool.Find("WarpCoreSocket"), Hands.RightHand, new Vector3(-0.1f, -0.01f, 0.03f), Quaternion.Euler(335f, 34f, 64f));
            Hands.HoldObject(itemCarryTool.Find("VesselCoreSocket"), Hands.RightHand, new Vector3(-0.07f, -0.03f, -0.01f), Quaternion.Euler(31.1f, 70.4f, 26f)).gameObject.AddComponent<DebugTransform>();
            Hands.HoldObject(itemCarryTool.Find("LanternSocket"), Hands.RightHand);
        }
    }
}
