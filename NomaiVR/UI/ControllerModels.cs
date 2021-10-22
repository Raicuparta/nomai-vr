using NomaiVR.Hands;
using NomaiVR.Helpers;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.UI
{
    internal class ControllerModels : NomaiVRModule<ControllerModels.Behaviour, NomaiVRModule.EmptyPatch>
    {

        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private SteamVR_RenderModel leftRenderModel;
            private SteamVR_RenderModel rightRenderModel;
            private bool isVisible;

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
                if (!isVisible && shouldShow)
                {
                    Show();
                }
                else if (isVisible && !shouldShow)
                {
                    Hide();
                }
            }

            private void SetUp()
            {
                leftRenderModel = CreateModel(HandsController.Behaviour.LeftHand, SteamVR_Input_Sources.LeftHand, SteamVR_Actions.default_LeftHand);
                rightRenderModel = CreateModel(HandsController.Behaviour.RightHand, SteamVR_Input_Sources.RightHand, SteamVR_Actions.default_RightHand);
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
                isVisible = true;
                HandsController.Behaviour.LeftHandBehaviour.SetLimitRangeOfMotion(true);
                leftRenderModel.gameObject.SetActive(true);
                HandsController.Behaviour.RightHandBehaviour.SetLimitRangeOfMotion(true);
                rightRenderModel.gameObject.SetActive(true);
            }

            private void Hide()
            {
                isVisible = false;
                HandsController.Behaviour.LeftHandBehaviour.SetLimitRangeOfMotion(false);
                leftRenderModel.gameObject.SetActive(false);
                HandsController.Behaviour.RightHandBehaviour.SetLimitRangeOfMotion(false);
                rightRenderModel.gameObject.SetActive(false);
            }
        }
    }
}
