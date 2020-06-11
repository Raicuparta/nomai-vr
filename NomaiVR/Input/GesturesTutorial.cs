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
                // TODO look into probe prompt trigger in other scene.
                var isShowingProbeText = IsShowing(TutorialText.Probe);
                if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Probe) || PlayerState.IsInsideShip())
                {
                    if (isShowingProbeText)
                    {
                        SetText("");
                    }
                    return;
                }
                var camera = Camera.main.transform;
                var hit = Physics.Raycast(camera.position, camera.forward, out var raycastHit, 75f, OWLayerMask.blockableInteractMask);
                if (!hit)
                {
                    return;
                }

                var promptReceiver = raycastHit.collider.GetComponent<ProbePromptReceiver>();
                if (!promptReceiver && isShowingProbeText)
                {
                    SetText("");
                }
                if (promptReceiver && !isShowingProbeText)
                {
                    SetText(TutorialText.Probe);
                }
            }

            private static void UpdateSignalscopePrompt(ScreenPrompt main, ScreenPrompt center, bool isPlayingHideAndSeek)
            {
                var isShowingText = IsShowing(TutorialText.Signalscope);
                var isMainVisibleHideAndSeek = main.IsVisible() && isPlayingHideAndSeek;
                var shouldShowText = center.IsVisible() || isMainVisibleHideAndSeek;
                if (!isShowingText && shouldShowText)
                {
                    SetText(TutorialText.Signalscope);
                }
                if (isShowingText && !shouldShowText)
                {
                    SetText(TutorialText.None);
                }
            }

            private static void UpdateFlashlightPrompt(ScreenPrompt main, ScreenPrompt center)
            {
                var isShowingText = IsShowing(TutorialText.Flashlight);
                var isMainVisbileDark = main.IsVisible() && PlayerState.InDarkZone();
                var shouldShowText = center.IsVisible() || isMainVisbileDark;
                if (!isShowingText && shouldShowText)
                {
                    SetText(TutorialText.Flashlight);
                }
                if (isShowingText && !shouldShowText)
                {
                    SetText(TutorialText.None);
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

                private static void PostToolModeUiUpdate(
                    ScreenPrompt ____flashlightPrompt,
                    ScreenPrompt ____centerFlashlightPrompt,
                    ScreenPrompt ____signalscopePrompt,
                    ScreenPrompt ____centerSignalscopePrompt,
                    bool ____playingHideAndSeek
                )
                {
                    UpdateFlashlightPrompt(____flashlightPrompt, ____centerFlashlightPrompt);
                    UpdateSignalscopePrompt(____signalscopePrompt, ____centerSignalscopePrompt, ____playingHideAndSeek);
                }
            }
        }

        private struct TutorialText
        {
            public static string None = "";
            public static string Probe = "Use right hand to grab probe launcher from tool belt (middle)";
            public static string Signalscope = "Use right hand to grab signalscope from tool belt (right side)";
            public static string Translator = "Use left hand to grab signalscope from tool belt (left side)";
            public static string Flashlight = "Touch right side of head with right hand to toggle flashlight";
        }
    }
}
