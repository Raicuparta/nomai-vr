using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class ControllerModels : NomaiVRModule<ControllerModels.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private SteamVR_RenderModel _leftRenderModel;
            private SteamVR_RenderModel _rightRenderModel;
            private bool _isVisible;

            internal void Start()
            {
                SetUp();
                Show();
            }

            internal void Update()
            {
                UpdateVisibility();
            }

            private void UpdateVisibility()
            {
                if (OWTime.IsPaused(OWTime.PauseType.Menu) || OWTime.IsPaused(OWTime.PauseType.Sleeping) || SceneHelper.IsInTitle())
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }

            private void SetUp()
            {
                _leftRenderModel = CreateModel(HandsController.Behaviour.LeftHand, SteamVR_Input_Sources.LeftHand, 1);
                _rightRenderModel = CreateModel(HandsController.Behaviour.RightHand, SteamVR_Input_Sources.RightHand, 2);
            }

            private SteamVR_RenderModel CreateModel(Transform parent, SteamVR_Input_Sources source, int index)
            {
                var model = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
                model.gameObject.layer = LayerMask.NameToLayer("UI");
                model.transform.parent = parent;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                model.SetInputSource(source);
                model.SetDeviceIndex(index);

                return model;
            }

            private void Show()
            {
                if (_isVisible)
                {
                    return;
                }
                _isVisible = true;
                _leftRenderModel.gameObject.SetActive(true);
                _rightRenderModel.gameObject.SetActive(true);
            }

            private void Hide()
            {
                if (!_isVisible)
                {
                    return;
                }
                _isVisible = false;
                _leftRenderModel.gameObject.SetActive(false);
                _rightRenderModel.gameObject.SetActive(false);
            }
        }
    }
}
