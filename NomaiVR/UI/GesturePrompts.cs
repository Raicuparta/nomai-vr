using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class GesturePrompts : NomaiVRModule<GesturePrompts.Behaviour, GesturePrompts.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Text _text;
            private Canvas _canvas;

            internal void Start()
            {
                SetUpCanvas();

                GlobalMessenger.AddListener("EnterProbePromptTrigger", OnEnterProbePromptTrigger);
                GlobalMessenger.AddListener("ExitProbePromptTrigger", OnExitProbePromptTrigger);
            }

            private void SetUpCanvas()
            {
                _canvas = new GameObject().AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.WorldSpace;
                var followTarget = _canvas.gameObject.AddComponent<FollowTarget>();
                _canvas.transform.localScale = Vector3.one * 0.0015f;
                followTarget.target = Locator.GetPlayerCamera().transform;
                followTarget.localPosition = Vector3.forward * 4;
                followTarget.rotationSmoothTime = 0.5f;
                followTarget.positionSmoothTime = 0.5f;
                _canvas.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = ShouldRender;

                SetUpBackground();
                SetUpText();

                MaterialHelper.MakeGraphicChildrenDrawOnTop(_canvas.gameObject);
                LayerHelper.ChangeLayerRecursive(_canvas.gameObject, LayerMask.NameToLayer("UI"));
            }

            private void SetUpBackground()
            {
                var size = new Vector3(13f, 2f, 1);

                // Background that draws on top of everything;
                var background = new GameObject().AddComponent<Image>();
                background.transform.SetParent(_canvas.transform, false);
                background.transform.localScale = size;
                background.transform.localPosition = Vector3.forward;
                background.color = Color.black;

                // Quad to block distortions on text;
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(_canvas.transform, false);
                quad.transform.localScale = size * 100;
                Destroy(quad.GetComponent<Collider>());
                var material = quad.GetComponent<MeshRenderer>().material;
                material.color = Color.black;
            }

            private void SetUpText()
            {
                _text = new GameObject().AddComponent<Text>();
                _text.color = Color.white;
                _text.transform.SetParent(_canvas.transform, false);
                _text.fontSize = 50;
                _text.font = FindObjectOfType<DialogueBoxVer2>().GetComponentInChildren<Text>().font;
                _text.verticalOverflow = VerticalWrapMode.Overflow;
                _text.horizontalOverflow = HorizontalWrapMode.Overflow;
                _text.alignment = TextAnchor.MiddleCenter;
            }

            private bool ShouldRender()
            {
                return ModSettings.EnableGesturePrompts && _text.text != GestureText.None;
            }

            private static void SetText(string text)
            {
                if (_text.text == text)
                {
                    return;
                }
                _text.text = text;
            }

            private void OnEnterProbePromptTrigger()
            {
                if (PlayerState.IsWearingSuit())
                {
                    SetText(GestureText.Probe);
                }
            }

            private void OnExitProbePromptTrigger()
            {
                SetText(GestureText.None);
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

                var isShowingTranslatorText = IsShowing(GestureText.Translator);
                var nomaiText = hit.collider.GetComponent<NomaiText>();
                if (!nomaiText && isShowingTranslatorText)
                {
                    SetText(GestureText.None);
                }
                if (nomaiText && !isShowingTranslatorText)
                {
                    SetText(GestureText.Translator);
                }
            }

            private void UpdateProbePrompt(RaycastHit hit)
            {
                var isShowingProbeText = IsShowing(GestureText.Probe);
                var promptReceiver = hit.collider.GetComponent<ProbePromptReceiver>();
                if (!promptReceiver && isShowingProbeText)
                {
                    SetText(GestureText.None);
                }
                if (PlayerState.IsWearingSuit() && promptReceiver && !isShowingProbeText)
                {
                    SetText(GestureText.Probe);
                }
            }

            private void UpdateRaycast()
            {
                if (ToolHelper.IsUsingAnyTool() || PlayerState.IsInsideShip())
                {
                    SetText(GestureText.None);
                    return;
                }

                var camera = Locator.GetPlayerCamera().transform;
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
                var isShowingText = IsShowing(GestureText.Signalscope);
                var isMainVisibleHideAndSeek = main.IsVisible() && isPlayingHideAndSeek;
                var shouldShowText = center.IsVisible() || isMainVisibleHideAndSeek;
                if (!isShowingText && shouldShowText)
                {
                    SetText(GestureText.Signalscope);
                }
                if (isShowingText && !shouldShowText)
                {
                    SetText(GestureText.None);
                }
            }

            private static void UpdateFlashlightPrompt(ScreenPrompt main, ScreenPrompt center)
            {
                var isShowingText = IsShowing(GestureText.Flashlight);
                if (PlayerState.IsFlashlightOn() && !isShowingText)
                {
                    return;
                }
                var tutorialStep = "flashlight";
                var hasUsedFlashlight = NomaiVR.Save.tutorialSteps.Contains(tutorialStep);
                var isMainVisbileDark = main.IsVisible() && PlayerState.InDarkZone();
                var shouldShowText = (center.IsVisible() || isMainVisbileDark) && !hasUsedFlashlight;
                if (!isShowingText && shouldShowText)
                {
                    SetText(GestureText.Flashlight);
                }
                if (isShowingText && !shouldShowText)
                {
                    SetText(GestureText.None);
                    if (PlayerState.IsFlashlightOn())
                    {
                        NomaiVR.Save.AddTutorialStep(tutorialStep);
                    }
                }
            }

            private static void UpdateTranslatorPrompt(ScreenPrompt prompt)
            {
                var isShowingText = IsShowing(GestureText.Translator);
                if (!isShowingText && prompt.IsVisible())
                {
                    SetText(GestureText.Translator);
                }
                if (isShowingText && !prompt.IsVisible())
                {
                    SetText(GestureText.None);
                }
            }

            private static void UpdateWakeUpPrompt()
            {
                var hand = HandsController.Behaviour.RightHand;
                var isLookingAtHand = hand != null && CameraHelper.IsOnScreen(hand.position);
                var isSleeping = OWTime.IsPaused(OWTime.PauseType.Sleeping);
                var shouldShowText = !isLookingAtHand && isSleeping;
                var isShowingText = IsShowing(GestureText.WakeUp);

                if (!isShowingText && shouldShowText)
                {
                    SetText(GestureText.WakeUp);
                }
                else if (isShowingText && !shouldShowText)
                {
                    SetText(GestureText.None);
                }
            }

            internal void LateUpdate()
            {
                if (!ModSettings.EnableGesturePrompts)
                {
                    return;
                }
                UpdateWakeUpPrompt();
                UpdateRaycast();
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ToolModeUI>("Update", nameof(PostToolModeUiUpdate));
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
                    if (!ModSettings.EnableGesturePrompts)
                    {
                        return;
                    }
                    UpdateFlashlightPrompt(____flashlightPrompt, ____centerFlashlightPrompt);
                    UpdateSignalscopePrompt(____signalscopePrompt, ____centerSignalscopePrompt, ____playingHideAndSeek);
                    UpdateTranslatorPrompt(____centerTranslatePrompt);
                }
            }
        }

        private static class GestureText
        {
            public static string None = "";
            public static string Probe = GetToolBeltPrompt("Probe Launcher", "Middle");
            public static string Signalscope = GetToolBeltPrompt("Signalscope", "Right");
            public static string Translator = GetToolBeltPrompt("Translator", "Left");
            public static string Flashlight = "Touch right side of head with right hand to toggle <color=orange>Flashlight</color>.";
            public static string WakeUp = "Look at your <color=orange>right hand</color>.";

            public static string GetToolBeltPrompt(string toolName, string slot)
            {
                return $"Grab <color=orange>{toolName}</color> from tool belt with right hand.\n({slot} slot.)";
            }
        }
    }
}
