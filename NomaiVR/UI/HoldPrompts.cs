using UnityEngine;

namespace NomaiVR
{
    internal class HoldPrompts : NomaiVRModule<HoldPrompts.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform _holdTransform;
            private Canvas _promptCanvas;
            private bool _isTranslatorPosition;

            internal void Start()
            {
                _promptCanvas = GameObject.Find("ScreenPromptCanvas").GetComponent<Canvas>();
                _promptCanvas.gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
                _promptCanvas.transform.localScale = Vector3.one * 0.0015f;

                _promptCanvas.renderMode = RenderMode.WorldSpace;
                _promptCanvas.transform.localPosition = Vector3.zero;
                _promptCanvas.transform.localRotation = Quaternion.identity;

                _holdTransform = new GameObject().transform;
                HandsController.Behaviour.DominantHandBehaviour.Initialized += ParentToDominantHand;

                _promptCanvas.transform.SetParent(_holdTransform, false);
                _promptCanvas.transform.localPosition = Vector3.down * 0.1f;
                _promptCanvas.transform.localRotation = Quaternion.identity;
                SetPositionToHand();

                foreach (Transform child in _promptCanvas.transform)
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
                    _holdTransform.SetParent(VRToolSwapper.InteractingHand.Palm, false);
                    UpdateHandPosition();
                }
                else
                    ParentToDominantHand();
            }

            internal void ParentToDominantHand()
            {
                Transform dominantHand = HandsController.Behaviour.DominantHandBehaviour.Palm;
                _holdTransform.SetParent(dominantHand, false);
                UpdateHandPosition();
            }

            internal void UpdateHandPosition()
            {
                if (_isTranslatorPosition)
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
                _holdTransform.LookAt(2 * _holdTransform.position - Camera.main.transform.position, PlayerHelper.PlayerHead.up);
            }

            private void UpdatePosition()
            {
                var isUsingTool = ToolHelper.Swapper.IsInToolMode(ToolMode.Translator, ToolGroup.Suit);
                if (!_isTranslatorPosition && isUsingTool)
                {
                    SetPositionToTranslator();
                }
                else if (_isTranslatorPosition && !isUsingTool)
                {
                    SetPositionToHand();
                }
            }

            private void SetPositionToHand()
            {
                bool isRightHanded = _holdTransform.parent == HandsController.Behaviour.RightHandBehaviour.Palm;
                _promptCanvas.transform.localPosition = Vector3.down * 0.1f;
                _isTranslatorPosition = false;
            }

            private void SetPositionToTranslator()
            {
                bool isRightHanded = _holdTransform.parent == HandsController.Behaviour.RightHandBehaviour.Palm;
                _promptCanvas.transform.localPosition = Vector3.down * 0.15f;
                _isTranslatorPosition = true;
            }
        }
    }
}