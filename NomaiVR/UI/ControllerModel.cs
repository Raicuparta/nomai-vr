using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR
{
    internal class ControllerModel : NomaiVRModule<ControllerModel.Behaviour, ControllerModel.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static List<ScreenPromptList> _worldPromptLists;

            internal void Update()
            {
                UpdateControllerModelVisibility();
            }

            private void ShowModel()
            {

            }

            private void HideModel()
            {

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
