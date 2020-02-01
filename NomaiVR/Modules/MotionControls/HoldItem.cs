using UnityEngine;

namespace NomaiVR {
    class HoldItem: MonoBehaviour {
        void Awake () {
            var itemCarryTool = GameObject.FindObjectOfType<ItemTool>();
            Hands.HoldObject(itemCarryTool.transform, Hands.RightHand);

            Destroy(GameObject.FindObjectOfType<FirstPersonManipulator>());
            Hands.RightHand.gameObject.AddComponent<FirstPersonManipulator>();
        }
    }
}
