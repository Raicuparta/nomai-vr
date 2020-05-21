using UnityEngine;

namespace NomaiVR
{
    public class HoldItem : NomaiVRModule<HoldItem.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private ItemTool _itemTool;

            private void Start()
            {
                _itemTool = FindObjectOfType<ItemTool>();
                _itemTool.transform.localScale = 1.8f * Vector3.one;

                _itemTool.transform.Find("ItemSocket").gameObject.AddComponent<Holdable>();
                _itemTool.transform.Find("LanternSocket").gameObject.AddComponent<Holdable>();

                var scroll = _itemTool.transform.Find("ScrollSocket").gameObject.AddComponent<Holdable>();
                scroll.transform.localPosition = new Vector3(0.02f, -0.04f, -0.03f);
                scroll.transform.localRotation = Quaternion.Euler(354f, 104f, 194f);

                var stone = _itemTool.transform.Find("SharedStoneSocket").gameObject.AddComponent<Holdable>();
                stone.transform.localPosition = new Vector3(-0.05f, -0.01f, 0f);
                stone.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

                var warpCore = _itemTool.transform.Find("WarpCoreSocket").gameObject.AddComponent<Holdable>();
                warpCore.transform.localPosition = new Vector3(-0.06f, -0.07f, -0.05f);
                warpCore.transform.localRotation = Quaternion.Euler(309f, 49f, 104f);

                var vesselCore = _itemTool.transform.Find("VesselCoreSocket").gameObject.AddComponent<Holdable>();
                vesselCore.transform.localPosition = new Vector3(-0.01f, 0.03f, 0.01f);
                vesselCore.transform.localRotation = Quaternion.Euler(31.1f, 70.4f, 26f);
            }

            private void SetActive(bool active)
            {
                var heldItem = _itemTool.GetHeldItem();
                if (!heldItem)
                {
                    return;
                }
                heldItem.gameObject.SetActive(active);
            }

            private bool IsActive()
            {
                var heldItem = _itemTool.GetHeldItem();
                if (!heldItem)
                {
                    return false;
                }
                return heldItem.gameObject.activeSelf;
            }

            private void Update()
            {
                if (IsActive() && ToolHelper.IsUsingAnyTool())
                {
                    SetActive(false);
                }
                else if (!IsActive() && !ToolHelper.IsUsingAnyTool())
                {
                    SetActive(true);
                }
            }
        }
    }
}