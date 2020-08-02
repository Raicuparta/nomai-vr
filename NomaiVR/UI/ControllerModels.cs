using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class ControllerModels : NomaiVRModule<ControllerModels.Behaviour, ControllerModels.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private static List<ScreenPromptList> _worldPromptLists;
            private SteamVR_RenderModel _leftRenderModel;
            private SteamVR_RenderModel _rightRenderModel;

            internal void Start()
            {
                SetUpModels();
                ShowModels();
            }

            internal void Update()
            {
                UpdateControllerModelVisibility();
            }

            private void SetUpModels()
            {
                _leftRenderModel = CreateModel(HandsController.Behaviour.LeftHand, SteamVR_Input_Sources.LeftHand, 1);
                _rightRenderModel = CreateModel(HandsController.Behaviour.RightHand, SteamVR_Input_Sources.RightHand, 2);
            }

            private SteamVR_RenderModel CreateModel(Transform parent, SteamVR_Input_Sources source, int index)
            {
                var model = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
                model.transform.parent = parent;
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localScale = Vector3.one;
                model.SetInputSource(source);
                model.SetDeviceIndex(index);

                return model;
            }

            private void ShowModels()
            {
                _leftRenderModel.gameObject.SetActive(true);
                _rightRenderModel.gameObject.SetActive(true);
            }

            private void HideModel()
            {
                _leftRenderModel.gameObject.SetActive(false);
                _rightRenderModel.gameObject.SetActive(false);
            }

            private void UpdateControllerModelVisibility()
            {
                var isTitleScene = LoadManager.GetCurrentScene() == OWScene.TitleScreen;
                if (isTitleScene || _worldPromptLists == null)
                {
                    ShowModels();
                    return;
                }
                foreach (var promptList in _worldPromptLists)
                {
                    if (promptList.IsDisplayingAnyPrompt())
                    {
                        NomaiVR.Log("show them models");
                        ShowModels();
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
