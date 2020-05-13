using UnityEngine;

namespace NomaiVR
{
    class HoldItem : MonoBehaviour
    {
        ItemTool _itemTool;

        void Awake()
        {
            _itemTool = FindObjectOfType<ItemTool>();
            _itemTool.transform.localScale = 1.8f * Vector3.one;
            HandsController.HoldObject(_itemTool.transform.Find("ItemSocket"), HandsController.RightHand);
            HandsController.HoldObject(_itemTool.transform.Find("ScrollSocket"), HandsController.RightHand, new Vector3(0.02f, -0.04f, -0.03f), Quaternion.Euler(354f, 104f, 194f));
            HandsController.HoldObject(_itemTool.transform.Find("SharedStoneSocket"), HandsController.RightHand, new Vector3(-0.05f, -0.01f, 0f), Quaternion.Euler(10f, 0f, 0f));
            HandsController.HoldObject(_itemTool.transform.Find("WarpCoreSocket"), HandsController.RightHand, new Vector3(-0.06f, -0.07f, -0.05f), Quaternion.Euler(309f, 49f, 104f));
            HandsController.HoldObject(_itemTool.transform.Find("VesselCoreSocket"), HandsController.RightHand, new Vector3(-0.01f, 0.03f, 0.01f), Quaternion.Euler(31.1f, 70.4f, 26f));
            HandsController.HoldObject(_itemTool.transform.Find("LanternSocket"), HandsController.RightHand);
        }

        void SetActive(bool active)
        {
            var heldItem = _itemTool.GetHeldItem();
            if (!heldItem)
            {
                return;
            }
            heldItem.gameObject.SetActive(active);
        }

        bool IsActive()
        {
            var heldItem = _itemTool.GetHeldItem();
            if (!heldItem)
            {
                return false;
            }
            return heldItem.gameObject.activeSelf;
        }

        void Update()
        {
            if (IsActive() && Common.IsUsingAnyTool())
            {
                SetActive(false);
            }
            else if (!IsActive() && !Common.IsUsingAnyTool())
            {
                SetActive(true);
            }
        }
    }
}
