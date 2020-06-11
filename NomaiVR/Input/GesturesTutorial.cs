using OWML.ModHelper.Events;
using System;
using System.Security.Policy;
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
                _text.font = FindObjectOfType<DialogueBoxVer2>().GetComponentInChildren<Text>().font;
                _text.verticalOverflow = VerticalWrapMode.Overflow;
                _text.horizontalOverflow = HorizontalWrapMode.Overflow;
                _text.alignment = TextAnchor.MiddleCenter;

                _text.material = new Material(_text.material);
                MaterialHelper.MakeMaterialDrawOnTop(_text.material);
            }

            private static void SetText(string text)
            {
                if (_text.text == text)
                {
                    return;
                }
                _text.text = text;
            }

            private static bool IsShowing(string text)
            {
                return _text.text == text;
            }

            private void UpdateTranslatorPrompt(RaycastHit hit)
            {
                if (PlayerData.GetPersistentCondition("HAS_USED_TRANSLATOR"))
                {
                    return;
                }

                var isShowingTranslatorText = IsShowing(TutorialText.Translator);
                var nomaiText = hit.collider.GetComponent<NomaiText>();
                if (!nomaiText && isShowingTranslatorText)
                {
                    SetText(TutorialText.None);
                }
                if (nomaiText && !isShowingTranslatorText)
                {
                    SetText(TutorialText.Translator);
                }
            }

            private void UpdateProbePrompt(RaycastHit hit)
            {
                // TODO look into probe prompt trigger in other scene.
                var isShowingProbeText = IsShowing(TutorialText.Probe);
                var promptReceiver = hit.collider.GetComponent<ProbePromptReceiver>();
                if (!promptReceiver && isShowingProbeText)
                {
                    SetText(TutorialText.None);
                }
                if (promptReceiver && !isShowingProbeText)
                {
                    SetText(TutorialText.Probe);
                }
            }

            private void UpdateRaycast()
            {
                if (ToolHelper.IsUsingAnyTool() || PlayerState.IsInsideShip())
                {
                    SetText(TutorialText.None);
                    return;
                }

                var camera = Camera.main.transform;
                var isHit = Physics.Raycast(camera.position, camera.forward, out var hit, 5f, OWLayerMask.blockableInteractMask);
                if (!isHit)
                {
                    return;
                }

                UpdateProbePrompt(hit);
                UpdateTranslatorPrompt(hit);
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

            private static void UpdateTranslatorPrompt(ScreenPrompt prompt)
            {
                var isShowingText = IsShowing(TutorialText.Translator);
                if (!isShowingText && prompt.IsVisible())
                {
                    SetText(TutorialText.Translator);
                }
                if (isShowingText && !prompt.IsVisible())
                {
                    SetText(TutorialText.None);
                }
            }

            internal void LateUpdate()
            {
                UpdateRaycast();
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
                    ScreenPrompt ____centerTranslatePrompt,
                    bool ____playingHideAndSeek
                )
                {
                    UpdateFlashlightPrompt(____flashlightPrompt, ____centerFlashlightPrompt);
                    UpdateSignalscopePrompt(____signalscopePrompt, ____centerSignalscopePrompt, ____playingHideAndSeek);
                    UpdateTranslatorPrompt(____centerTranslatePrompt);
                }
            }
        }

        private static class TutorialText
        {
            public static string None = "";
            public static string Probe = GetToolBeltPrompt("Probe Launcher", "Middle");
            public static string Signalscope = GetToolBeltPrompt("Signalscope", "Right");
            public static string Translator = GetToolBeltPrompt("Translator", "Left");
            public static string Flashlight = "Touch right side of head with right hand to toggle Flashlight.";

            public static string GetToolBeltPrompt(string toolName, string slot)
            {
                return $"Grab <color=orange>{toolName}</color> from tool belt with right hand.\n({slot} slot.)";
            }
        }
    }
}
