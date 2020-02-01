using UnityEngine;

namespace NomaiVR {
    class HoldItem: MonoBehaviour {
        void Awake () {
            var itemCarryTool = GameObject.FindObjectOfType<ItemTool>();
            itemCarryTool.transform.localScale = 1.8f * Vector3.one;
            Hands.HoldObject(itemCarryTool.transform, Hands.RightHand, new Vector3(-0.17f, 0.08f, -0.54f), Quaternion.Euler(340, 16.41602f, 280));
        }
    }
}
