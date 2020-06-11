using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class GesturesTutorial : NomaiVRModule<GesturesTutorial.Behaviour, GesturesTutorial.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Text _text;

            internal void Start()
            {
                SetUpText();
            }

            private void SetUpText()
            {
                var canvas = new GameObject().AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                canvas.transform.localScale = Vector3.one * 0.002f;
                followTarget.target = Locator.GetPlayerCamera().transform;
                followTarget.localPosition = Vector3.forward * 4;
                followTarget.rotationSmoothTime = 0.5f;
                followTarget.positionSmoothTime = 0.5f;

                _text = new GameObject().AddComponent<Text>();
                _text.color = Color.white;
                _text.transform.SetParent(canvas.transform, false);
                _text.fontSize = 50;
                _text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                _text.horizontalOverflow = HorizontalWrapMode.Overflow;
                _text.alignment = TextAnchor.MiddleCenter;

                _text.material = new Material(_text.material);
                MaterialHelper.MakeMaterialDrawOnTop(_text.material);
            }

            private static void SetText(string text)
            {
                _text.text = text;
            }

            private static bool IsShowing(string text)
            {
                return _text.text == text;
            }

            private void UpdateProbePrompt()
            {
                var camera = Camera.main.transform;
                var hit = Physics.Raycast(camera.position, camera.forward, out var raycastHit, 75f, OWLayerMask.blockableInteractMask);
                if (!hit)
                {
                    return;
                }

                var promptReceiver = raycastHit.collider.GetComponent<ProbePromptReceiver>();
                var isShowingProbeText = IsShowing(TutorialText.Probe);
                if (!promptReceiver && isShowingProbeText)
                {
                    SetText("");
                }
                if (promptReceiver && !isShowingProbeText)
                {
                    SetText("Grab probe launcher from tool belt");
                }
            }

            internal void LateUpdate()
            {
                UpdateProbePrompt();
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ToolModeUI>("Update", typeof(Patch), nameof(PostToolModeUiUpdate));
                }

                private static void PostToolModeUiUpdate(ScreenPrompt ____centerFlashlightPrompt, ScreenPrompt ____flashlightPrompt)
                {
                    var isShowingFlashlightText = IsShowing(TutorialText.Flashlight);
                    var isMainPromptVisbileInDark = ____flashlightPrompt.IsVisible() && PlayerState.InDarkZone();
                    var shouldShowTutorial = ____centerFlashlightPrompt.IsVisible() || isMainPromptVisbileInDark;
                    if (!isShowingFlashlightText && shouldShowTutorial)
                    {
                        SetText(TutorialText.Flashlight);
                    }
                    if (isShowingFlashlightText && !shouldShowTutorial)
                    {
                        SetText(TutorialText.None);
                    }
                }
            }
        }

        private struct TutorialText
        {
            public static string None = "";
            public static string Probe = "Grab probe launcher from tool belt";
            public static string Flashlight = "Touch right side of head to toggle flashlight";
        }
    }
}
