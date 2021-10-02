using UnityEngine;

namespace NomaiVR
{
    internal class HoldItem : NomaiVRModule<HoldItem.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private ItemTool _itemTool;

            internal void Start()
            {
                _itemTool = FindObjectOfType<ItemTool>();
                _itemTool.transform.localScale = 1.8f * Vector3.one;

                // Used by wordstones.
                // TODO This doesn't seem to exist anymore. Go to quantum moon and check what's up.
                // var wordStone = _itemTool.transform.Find("ItemSocket").gameObject.AddComponent<Holdable>();
                // wordStone.SetPositionOffset(new Vector3(-0.1652f, 0.0248f, 0.019f), new Vector3(-0.1804f, 0.013f, 0.019f));
                // wordStone.SetRotationOffset(Quaternion.Euler(0f, -90f, 10f));
                // wordStone.SetPoses(AssetLoader.Poses["holding_wordstone"]);

                _itemTool.transform.Find("SimpleLanternSocket").gameObject.AddComponent<Holdable>();
                _itemTool.transform.Find("DreamLanternSocket").gameObject.AddComponent<Holdable>();
                _itemTool.transform.Find("SlideReelSocket").gameObject.AddComponent<Holdable>();
                _itemTool.transform.Find("VisionTorchSocket").gameObject.AddComponent<Holdable>();

                var scroll = _itemTool.transform.Find("ScrollSocket").gameObject.AddComponent<Holdable>();
                scroll.SetPositionOffset(new Vector3(-0.022f, -0.033f, -0.03f), new Vector3(-0.0436f, -0.033f, -0.03f));
                scroll.SetRotationOffset(Quaternion.Euler(352.984f, 97.98601f, 223.732f));
                scroll.SetPoses(AssetLoader.Poses["holding_scroll_gloves"], AssetLoader.Poses["holding_scroll_gloves"]);

                var stone = _itemTool.transform.Find("SharedStoneSocket").gameObject.AddComponent<Holdable>();
                stone.SetPositionOffset(new Vector3(-0.1139f, -0.0041f, 0.0193f));
                stone.SetRotationOffset(Quaternion.Euler(-22.8f, 0f, 0f));
                stone.SetPoses(AssetLoader.Poses["holding_sharedstone"], AssetLoader.Poses["holding_sharedstone_gloves"]);

                var warpCore = _itemTool.transform.Find("WarpCoreSocket").gameObject.AddComponent<Holdable>();
                warpCore.SetPositionOffset(new Vector3(-0.114f, -0.03f, -0.021f));
                warpCore.SetRotationOffset(Quaternion.Euler(-10f, 0f, 90f));
                warpCore.SetPoses(AssetLoader.Poses["holding_warpcore"], AssetLoader.Poses["holding_warpcore_gloves"]);

                //Comically warped pose, but it's the only way to hold this thing...
                var vesselCore = _itemTool.transform.Find("VesselCoreSocket").gameObject.AddComponent<Holdable>();
                vesselCore.SetPositionOffset(new Vector3(-0.0594f, 0.03f, 0.0256f));
                vesselCore.SetRotationOffset(Quaternion.Euler(-1.7f, 70.4f, 26f));
                vesselCore.SetPoses(AssetLoader.Poses["holding_vesselcore_gloves"], AssetLoader.Poses["holding_vesselcore_gloves"]);
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

            internal void Update()
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