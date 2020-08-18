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
            private bool _isToolMode;

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
                _holdTransform.SetParent(HandsController.Behaviour.RightHand, false);
                _holdTransform.gameObject.AddComponent<DebugTransform>();

                canvas.transform.SetParent(_holdTransform, false);
                SetPositionToHand();

                foreach (Transform child in canvas.transform)
                {
                    child.localPosition = Vector3.zero;
                }
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
                var isUsingTool = ToolHelper.IsUsingAnyTool(ToolGroup.Suit);
                if (!_isToolMode && isUsingTool)
                {
                    SetPositionToTool();
                }
                else if (_isToolMode && !isUsingTool)
                {
                    SetPositionToHand();
                }
            }

            private void SetPositionToHand()
            {
                _holdTransform.localPosition = new Vector3(-0.09f, -0.11f, 0.13f);
                _isToolMode = false;
            }

            private void SetPositionToTool()
            {
                _holdTransform.localPosition = new Vector3(-0.17f, 0.07f, -0.11f);
                _isToolMode = true;
            }
        }
    }
}