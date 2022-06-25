using NomaiVR.Hands;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.Tools;
using UnityEngine;

namespace NomaiVR.UI
{
    internal class HoldPrompts : NomaiVRModule<HoldPrompts.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform holdTransform;
            private Canvas promptCanvas;
            private bool isTranslatorPosition;

            internal void Start()
            {
                promptCanvas = GameObject.Find("ScreenPromptCanvas").GetComponent<Canvas>();
                promptCanvas.gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
                promptCanvas.transform.localScale = Vector3.one * 0.0015f;

                promptCanvas.renderMode = RenderMode.WorldSpace;
                promptCanvas.transform.localPosition = Vector3.zero;
                promptCanvas.transform.localRotation = Quaternion.identity;

                holdTransform = new GameObject("VrHoldPrompt").transform;
                HandsController.Behaviour.DominantHandBehaviour.Initialized += ParentToDominantHand;

                promptCanvas.transform.SetParent(holdTransform, false);
                promptCanvas.transform.localPosition = Vector3.down * 0.1f;
                promptCanvas.transform.localRotation = Quaternion.identity;
                SetPositionToHand();

                foreach (Transform child in promptCanvas.transform)
                {
                    child.localPosition = Vector3.zero;
                }

                ModSettings.OnConfigChange += ParentToDominantHand;
                VRToolSwapper.Equipped += ParentToInteractingHand;
                VRToolSwapper.UnEquipped += ParentToDominantHand;
            }

            internal void OnDestroy()
            {
                ModSettings.OnConfigChange -= ParentToDominantHand;
                VRToolSwapper.Equipped -= ParentToInteractingHand;
                VRToolSwapper.UnEquipped -= ParentToDominantHand;
            }

            internal void ParentToInteractingHand()
            {
                if (VRToolSwapper.InteractingHand != null)
                {
                    holdTransform.SetParent(VRToolSwapper.InteractingHand.Palm, false);
                    UpdateHandPosition();
                }
                else
                    ParentToDominantHand();
            }

            internal void ParentToDominantHand()
            {
                var dominantHand = HandsController.Behaviour.DominantHandBehaviour.Palm;
                holdTransform.SetParent(dominantHand, false);
                UpdateHandPosition();
            }

            internal void UpdateHandPosition()
            {
                if (isTranslatorPosition)
                    SetPositionToTranslator();
                else
                    SetPositionToHand();
            }

            internal void Update()
            {
                UpdateRotation();
                UpdatePosition();
            }

            private void UpdateRotation()
            {
                if (!Camera.main)
                {
                    return;
                }
                holdTransform.LookAt(2 * holdTransform.position - Camera.main.transform.position, PlayerHelper.PlayerHead.up);
            }

            private void UpdatePosition()
            {
                var isUsingTool = ToolHelper.Swapper.IsInToolMode(ToolMode.Translator, ToolGroup.Suit);
                if (!isTranslatorPosition && isUsingTool)
                {
                    SetPositionToTranslator();
                }
                else if (isTranslatorPosition && !isUsingTool)
                {
                    SetPositionToHand();
                }
            }

            private void SetPositionToHand()
            {
                var isRightHanded = holdTransform.parent == HandsController.Behaviour.RightHandBehaviour.Palm;
                promptCanvas.transform.localPosition = new Vector3(-0.1f, -0.05f, 0.1f);
                isTranslatorPosition = false;
            }

            private void SetPositionToTranslator()
            {
                var isRightHanded = holdTransform.parent == HandsController.Behaviour.RightHandBehaviour.Palm;
                promptCanvas.transform.localPosition = Vector3.down * 0.1f;
                isTranslatorPosition = true;
            }
        }
    }
}