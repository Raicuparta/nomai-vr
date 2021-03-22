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
                Hide();
            }

            internal void Update()
            {
                UpdateVisibility();
            }

            private void UpdateVisibility()
            {
                var shouldShow = !SteamVR_Input.isStartupFrame && IsPaused();
                if (!_isVisible && shouldShow)
                {
                    Show();
                }
                else if (_isVisible && !shouldShow)
                {
                    Hide();
                }
            }

            private void SetUp()
            {
                _leftRenderModel = CreateModel(HandsController.Behaviour.LeftHand, SteamVR_Input_Sources.LeftHand, SteamVR_Actions.default_LeftHand);
                _rightRenderModel = CreateModel(HandsController.Behaviour.RightHand, SteamVR_Input_Sources.RightHand, SteamVR_Actions.default_RightHand);
            }

            private bool IsPaused()
            {
                return OWTime.IsPaused(OWTime.PauseType.Menu)
                    || OWTime.IsPaused(OWTime.PauseType.Sleeping)
                    || SceneHelper.IsInTitle();
            }

            private SteamVR_RenderModel CreateModel(Transform parent, SteamVR_Input_Sources source, SteamVR_Action_Pose pose)
            {
                var model = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
                model.gameObject.layer = LayerMask.NameToLayer("UI");
                model.transform.parent = parent;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                model.SetInputSource(source);
                model.SetDeviceIndex((int)pose.GetDeviceIndex(source));

                return model;
            }

            private void Show()
            {
                _isVisible = true;
                HandsController.Behaviour.LeftHandBehaviour.SetLimitRangeOfMotion(true);
                _leftRenderModel.gameObject.SetActive(true);
                HandsController.Behaviour.RightHandBehaviour.SetLimitRangeOfMotion(true);
                _rightRenderModel.gameObject.SetActive(true);
            }

            private void Hide()
            {
                _isVisible = false;
                HandsController.Behaviour.LeftHandBehaviour.SetLimitRangeOfMotion(false);
                _leftRenderModel.gameObject.SetActive(false);
                HandsController.Behaviour.RightHandBehaviour.SetLimitRangeOfMotion(false);
                _rightRenderModel.gameObject.SetActive(false);
            }
        }
    }
}
