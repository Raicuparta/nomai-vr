using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class ControllerModel : NomaiVRModule<ControllerModel.Behaviour, ControllerModel.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private static List<ScreenPromptList> _worldPromptLists;
            private SteamVR_RenderModel _renderModel;

            internal void Start()
            {
                SetUpModel();
                ShowModel();
            }

            internal void Update()
            {
                //UpdateControllerModelVisibility();
            }

            private void SetUpModel()
            {
                _renderModel = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
                _renderModel.transform.parent = HandsController.Behaviour.LeftHand;
                _renderModel.transform.localPosition = Vector3.zero;
                _renderModel.transform.localRotation = Quaternion.identity;
                _renderModel.transform.localScale = Vector3.one;

                _renderModel.SetInputSource(SteamVR_Input_Sources.LeftHand);
                _renderModel.SetDeviceIndex(1);
            }

            private void ShowModel()
            {
                _renderModel.gameObject.SetActive(true);
            }

            private void HideModel()
            {
                _renderModel.gameObject.SetActive(false);
            }

            private void UpdateControllerModelVisibility()
            {
                if (_worldPromptLists == null)
                {
                    return;
                }
                foreach (var promptList in _worldPromptLists)
                {
                    if (promptList.IsDisplayingAnyPrompt())
                    {
                        ShowModel();
                        return;
                    }
                }
                HideModel();
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<PromptManager>("Awake", nameof(PostPromptManagerAwake));
                }

                private static void PostPromptManagerAwake(List<ScreenPromptList> ____worldPromptLists)
                {
                    _worldPromptLists = ____worldPromptLists;
                }
            }
        }
    }
}
