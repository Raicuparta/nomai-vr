using UnityEngine;

namespace NomaiVR {
    class HoldItem: MonoBehaviour {
        void Awake () {
            var itemCarryTool = GameObject.FindObjectOfType<ItemTool>().transform;
            itemCarryTool.localScale = 1.8f * Vector3.one;
            Hands.HoldObject(itemCarryTool.Find("ScrollSocket"), Hands.RightHand, new Vector3(-0.06f, -0.06f, -0.03f), Quaternion.Euler(354f, 104f, 194f)).gameObject.AddComponent<DebugTransform>();
            Hands.HoldObject(itemCarryTool.Find("ItemSocket"), Hands.RightHand);
            Hands.HoldObject(itemCarryTool.Find("SharedStoneSocket"), Hands.RightHand, new Vector3(-0.13f, -0.03f, 0f));
            Hands.HoldObject(itemCarryTool.Find("WarpCoreSocket"), Hands.RightHand, new Vector3(-0.1f, -0.01f, 0.03f), Quaternion.Euler(335f, 34f, 64f));
            Hands.HoldObject(itemCarryTool.Find("VesselCoreSocket"), Hands.RightHand, new Vector3(-0.1f, -0.01f, 0.03f), Quaternion.Euler(335f, 34f, 64f));
            Hands.HoldObject(itemCarryTool.Find("LanternSocket"), Hands.RightHand);
        }

        void DoIt () {
            Common.PlayerBody.transform.position = GameObject.Find("Props_NOM_WarpCoreBlack (2)").transform.position;
            Common.PlayerBody.GetAttachedOWRigidbody().SetVelocity(Vector3.zero);

        }
    }
}
