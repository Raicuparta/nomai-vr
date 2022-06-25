using NomaiVR.Hands;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.UI
{
    internal class GesturePrompts : NomaiVRModule<GesturePrompts.Behaviour, GesturePrompts.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        private const string flashLightTutorialStep = "flashlight";
        private const string hasUsedTranslatorCondition = "HAS_USED_TRANSLATOR";

        public class Behaviour : MonoBehaviour
        {
            private static Text text;
            private Canvas canvas;

            internal void Start()
            {
                SetUpCanvas();

                GlobalMessenger.AddListener("EnterProbePromptTrigger", OnEnterProbePromptTrigger);
                GlobalMessenger.AddListener("ExitProbePromptTrigger", OnExitProbePromptTrigger);
            }

            internal void OnDestroy()
            {
                GlobalMessenger.RemoveListener("EnterProbePromptTrigger", OnEnterProbePromptTrigger);
                GlobalMessenger.RemoveListener("ExitProbePromptTrigger", OnExitProbePromptTrigger);
            }

            private void SetUpCanvas()
            {
                canvas = new GameObject("VrGesturePromptCanvas").AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                canvas.transform.localScale = Vector3.one * 0.0015f;
                followTarget.Target = Locator.GetPlayerCamera().transform;
                followTarget.LocalPosition = Vector3.forward * 4;
                followTarget.RotationSmoothTime = 0.5f;
                followTarget.PositionSmoothTime = 0.5f;
                canvas.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = ShouldRender;

                SetUpBackground();
                SetUpText();

                MaterialHelper.MakeGraphicChildrenDrawOnTop(canvas.gameObject);
                LayerHelper.ChangeLayerRecursive(canvas.gameObject, LayerMask.NameToLayer("UI"));
            }

            private void SetUpBackground()
            {
                var size = new Vector3(13f, 2f, 1);

                // Background that draws on top of everything;
                var background = new GameObject("VrGesturePromptBackground").AddComponent<Image>();
                background.transform.SetParent(canvas.transform, false);
                background.transform.localScale = size;
                background.transform.localPosition = Vector3.forward;
                background.color = Color.black;

                // Quad to block distortions on text;
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.SetParent(canvas.transform, false);
                quad.transform.localScale = size * 100;
                Destroy(quad.GetComponent<Collider>());
                var material = quad.GetComponent<MeshRenderer>().material;
                material.color = Color.black;
            }

            private void SetUpText()
            {
                text = new GameObject("VrGesturePromptText").AddComponent<Text>();
                text.color = Color.white;
                text.transform.SetParent(canvas.transform, false);
                text.fontSize = 50;
                text.font = FindObjectOfType<DialogueBoxVer2>().GetComponentInChildren<Text>().font;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.alignment = TextAnchor.MiddleCenter;
            }

            private bool ShouldRender()
            {
                return ModSettings.EnableGesturePrompts && text.text != GestureText.None;
            }

            private static void SetText(string newText)
            {
                if (text.text == newText)
                {
                    return;
                }
                text.text = newText;
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
                return Behaviour.text.text == text;
            }

            private void UpdateTranslatorPrompt(RaycastHit hit)
            {
                if (PlayerData.GetPersistentCondition(hasUsedTranslatorCondition))
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
                var hasUsedFlashlight = NomaiVR.Save.TutorialSteps.Contains(flashLightTutorialStep);
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
                        NomaiVR.Save.AddTutorialStep(flashLightTutorialStep);
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
                var hand = HandsController.Behaviour.DominantHand;
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
                    Postfix<ToolModeUI>(nameof(ToolModeUI.Update), nameof(PostToolModeUiUpdate));
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
            public const string None = "";
            public static readonly string Probe = GetToolBeltPrompt("Probe Launcher", "Middle");
            public static readonly string Signalscope = GetToolBeltPrompt("Signalscope", "Right");
            public static readonly string Translator = GetToolBeltPrompt("Translator", "Left");
            public const string Flashlight = "Touch the side of your head to toggle <color=orange>Flashlight</color>. Press <color=" + TextHelper.Blue + ">Grip</color> to toggle it again.";
            public const string WakeUp = "Look at your <color=orange>main hand</color>.";

            private static string GetToolBeltPrompt(string toolName, string slot)
            {
                return $"Grab <color=orange>{toolName}</color> from tool belt\n({slot} slot.)";
            }
        }
    }
}
