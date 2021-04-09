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
            private bool _isTranslatorPosition;
            private const float _canvasSizeX = 0.1f;

            internal void Start()
            {
                var canvas = GameObject.Find("ScreenPromptCanvas").GetComponent<Canvas>();
                canvas.gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
                canvas.transform.localScale = Vector3.one * 0.0015f;
                canvas.transform.localPosition = Vector3.zero;
                canvas.transform.localRotation = Quaternion.identity;

                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.localPosition = Vector3.zero;
                canvas.transform.localRotation = Quaternion.identity;

                _holdTransform = new GameObject().transform;
                ParentToDominantHand();

                canvas.transform.SetParent(_holdTransform, false);
                SetPositionToHand();

                foreach (Transform child in canvas.transform)
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
                    _holdTransform.SetParent(VRToolSwapper.InteractingHand.transform, false);
                    UpdateHandPosition();
                }
                else
                    ParentToDominantHand();
            }

            internal void ParentToDominantHand()
            {
                Transform dominantHand = HandsController.Behaviour.DominantHand;
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
                bool isRightHanded = _holdTransform.parent == HandsController.Behaviour.RightHand;
                _holdTransform.localPosition = Quaternion.Euler(-32.8f, 0, 0) * new Vector3(isRightHanded ? -0.09f : 0.09f + _canvasSizeX, -0.11f, 0.13f);
                _isTranslatorPosition = false;
            }

            private void SetPositionToTranslator()
            {
                bool isRightHanded = _holdTransform.parent == HandsController.Behaviour.RightHand;
                _holdTransform.localPosition = Quaternion.Euler(-32.8f, 0, 0) * new Vector3(isRightHanded ? -0.17f : 0.17f + _canvasSizeX, 0.07f, -0.11f);
                _isTranslatorPosition = true;
            }
        }
    }
}