using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
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
                
                HoldWordStone();
                HoldSimpleLantern();
                HoldDreamLantern();
                HoldSlideReel();
                HoldVisionTorch();
                HoldScroll();
                HoldSharedStone();
                HoldWarpCore();
                HoldVesselCore();
            }

            private void HoldWordStone()
            { 
                var holdable = MakeSocketHoldable("ItemSocket");
                if (holdable == null) return; 
                holdable.SetPositionOffset(new Vector3(-0.1652f, 0.0248f, 0.019f), new Vector3(-0.1804f, 0.013f, 0.019f));
                holdable.SetRotationOffset(Quaternion.Euler(0f, -90f, 10f));
                holdable.SetPoses("holding_wordstone");
            }

            private void HoldSimpleLantern()
            {
                var holdable = MakeSocketHoldable("SimpleLanternSocket");
                if (holdable == null) return;
                // TODO: Poses.
            }

            private void HoldDreamLantern()
            {
                var holdable = MakeSocketHoldable("DreamLanternSocket");
                if (holdable == null) return;
                holdable.SetPositionOffset(new Vector3(-0.505f, 0.094f, 0.017f), new Vector3(-0.52f, 0.094f, 0.029f));
                holdable.SetRotationOffset(Quaternion.Euler(0f, 0f, -100.5f));
                holdable.SetPoses("holding_dreamlantern", "holding_dreamlantern_gloves");

                SetupUsableItem(holdable, 0.15f, 0.05f, new Vector3(0, 0.6f, 0.1f),
                                onEnter: (Transform hand) =>
                                {
                                    var lanternController = holdable.GetComponentInChildren<DreamLanternController>(false);
                                    if (lanternController != null && lanternController.IsHeldByPlayer())
                                    {
                                        NewControllerInput.SimulateInput(InputConsts.InputCommandType.TOOL_PRIMARY, true);
                                    }
                                },
                                onExit: (Transform hand) =>
                                {
                                    NewControllerInput.SimulateInput(InputConsts.InputCommandType.TOOL_PRIMARY, false);
                                });
            }

            private void HoldSlideReel()
            {
                var holdable = MakeSocketHoldable("SlideReelSocket");
                if (holdable == null) return;
                // TODO: Poses.
            }

            private void HoldVisionTorch()
            {
                var holdable = MakeSocketHoldable("VisionTorchSocket");
                if (holdable == null) return;
                // TODO: Poses.

                SetupUsableItem(holdable, 0.15f, 0.05f, Vector3.zero,
                                onEnter: (Transform hand) =>
                                {
                                    var torchItem = holdable.GetComponentInChildren<VisionTorchItem>(false);
                                    if (torchItem != null && torchItem._visible)
                                    {
                                        NewControllerInput.SimulateInput(InputConsts.InputCommandType.TOOL_PRIMARY, true);
                                    }
                                },
                                onExit: (Transform hand) =>
                                {
                                    NewControllerInput.SimulateInput(InputConsts.InputCommandType.TOOL_PRIMARY, false);
                                });
            }

            private void HoldScroll()
            {
                var holdable = MakeSocketHoldable("ScrollSocket");
                if (holdable == null) return;
                holdable.SetPositionOffset(new Vector3(-0.022f, -0.033f, -0.03f), new Vector3(-0.0436f, -0.033f, -0.03f));
                holdable.SetRotationOffset(Quaternion.Euler(352.984f, 97.98601f, 223.732f));
                holdable.SetPoses("holding_scroll_gloves", "holding_scroll_gloves");
            }

            private void HoldSharedStone()
            {
                var holdable = MakeSocketHoldable("SharedStoneSocket");
                if (holdable == null) return;
                holdable.SetPositionOffset(new Vector3(-0.1139f, -0.0041f, 0.0193f));
                holdable.SetRotationOffset(Quaternion.Euler(-22.8f, 0f, 0f));
                holdable.SetPoses("holding_sharedstone", "holding_sharedstone_gloves");
            }

            private void HoldWarpCore()
            {
                var holdable = MakeSocketHoldable("WarpCoreSocket");
                if (holdable == null) return;
                holdable.SetPositionOffset(new Vector3(-0.114f, -0.03f, -0.021f));
                holdable.SetRotationOffset(Quaternion.Euler(-10f, 0f, 90f));
                holdable.SetPoses("holding_warpcore", "holding_warpcore_gloves");
            }

            private void HoldVesselCore()
            {
                //Comically warped pose, but it's the only way to hold this thing...
                var holdable = MakeSocketHoldable("VesselCoreSocket");
                if (holdable == null) return;
                holdable.SetPositionOffset(new Vector3(-0.0594f, 0.03f, 0.0256f));
                holdable.SetRotationOffset(Quaternion.Euler(-1.7f, 70.4f, 26f));
                holdable.SetPoses("holding_vesselcore_gloves", "holding_vesselcore_gloves");
            }
            
            private Holdable MakeSocketHoldable(string socketName)
            {
                var socketTransform = _itemTool.transform.Find(socketName);
                if (socketTransform != null) return socketTransform.gameObject.AddComponent<Holdable>();
                Logs.WriteError($"Could not find socket with name {socketName}");
                return null;
            }

            private void SetupUsableItem(Holdable holdable, float useDistance, float useExitThreshold, Vector3 useOffest, 
                                            System.Action<Transform> onEnter, System.Action<Transform> onExit)
            {
                var itemInteractor = holdable.gameObject.AddComponent<ProximityDetector>();
                itemInteractor.LocalOffset = useOffest;
                itemInteractor.enabled = false;
                itemInteractor.MinDistance = useDistance;
                itemInteractor.ExitThreshold = useExitThreshold;
                itemInteractor.Other = HandsController.Behaviour.OffHand;
                itemInteractor.OnEnter += onEnter;
                itemInteractor.OnExit += onExit;
                holdable.OnHoldStateChanged += (holden) =>
                {
                    itemInteractor.enabled = holden;
                };
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

            internal void OnDestroy()
            {

            }
        }
    }
}