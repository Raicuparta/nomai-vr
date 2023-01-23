using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Tools
{
    internal class HoldTranslator : NomaiVRModule<HoldTranslator.Behaviour, HoldTranslator.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform translatorBeams;
            private NomaiTranslator nomaiTranslator;
            private MeshRenderer originalLeftArrowRenderer;
            private MeshRenderer originalRightArrowRenderer;
            private List<TouchButton> handheldButtons;
            private NomaiTranslatorProp translatorProp;

            internal void Start()
            {
                var translator = SetUpTranslator();
                translatorProp = translator.GetComponent<NomaiTranslatorProp>();
                originalLeftArrowRenderer = translatorProp._leftPageArrowRenderer;
                originalRightArrowRenderer = translatorProp._rightPageArrowRenderer;
                var holdable = SetUpHoldable(translator);
                var translatorGroup = SetUpTranslatorGroup(translator);
                var translatorModel = SetUpTranslatorModel(translatorGroup);
                SetUpLaser(translator);
                RemoveTextMaterials(translator);
                SetUpHolster(translatorModel);
                SetUpLaser(translator);
                SetUpTranslatorButtons(translator);

                holdable.OnFlipped += (isRight) =>
                {
                    var tagetScale = Mathf.Abs(translatorBeams.localScale.x);
                    if (!isRight) tagetScale *= -1;
                    translatorBeams.localScale = new Vector3(tagetScale, translatorBeams.localScale.y, translatorBeams.localScale.z);

                    translatorProp.TurnOffArrowEmission();

                    translatorProp._leftPageArrowRenderer = isRight ? originalLeftArrowRenderer : originalRightArrowRenderer;
                    translatorProp._rightPageArrowRenderer = isRight ? originalRightArrowRenderer : originalLeftArrowRenderer;

                    if (isRight) 
                        handheldButtons.ForEach(b => b.ResetInputs());
                    else 
                        handheldButtons.ForEach(b => b.MirrorInputs());

                    translatorProp.SetNomaiAudioArrowEmissions();
                };
            }

            private Transform SetUpTranslator()
            {
                var translator = Locator.GetPlayerCamera().transform.Find("NomaiTranslatorProp");
                nomaiTranslator = translator.GetComponent<NomaiTranslator>();
                translator.localScale = Vector3.one * 0.3f;
                return translator;
            }

            private Holdable SetUpHoldable(Transform translator)
            {
                var holdTranslator = translator.gameObject.AddComponent<Holdable>();
                holdTranslator.SetPositionOffset(new Vector3(-0.2019f, 0.1323f, 0.0451f), new Vector3(-0.209f, 0.1396f, 0.0451f));
                holdTranslator.SetPoses("grabbing_translator", "grabbing_translator_gloves");
                holdTranslator.CanFlipX = true;
                return holdTranslator;
            }

            private Transform SetUpTranslatorGroup(Transform translator)
            {
                var translatorGroup = translator.Find("TranslatorGroup");
                translatorGroup.localPosition = Vector3.zero;
                translatorGroup.localRotation = Quaternion.identity;
                translatorBeams = translatorGroup.Find("TranslatorBeams");
                translatorBeams.localScale = Vector3.one / 0.3f;
                return translatorGroup;
            }

            private Transform SetUpTranslatorModel(Transform translatorGroup)
            {
                var translatorModel = translatorGroup.Find("Props_HEA_Translator");
                translatorModel.localPosition = Vector3.zero;
                translatorModel.localRotation = Quaternion.identity;

                // This child seems to be only for some kind of shader effect.
                // Disabling it since it looks glitchy and doesn't seem necessary.
                translatorModel.Find("Props_HEA_Translator_Prepass").gameObject.SetActive(false);

                var renderers = translatorModel.gameObject.GetComponentsInChildren<MeshRenderer>(true);

                foreach (var renderer in renderers)
                {
                    foreach (var material in renderer.materials)
                    {
                        material.shader = Shader.Find("Standard");
                    }
                }

                return translatorModel;
            }

            private Transform SetUpLaser(Transform translator)
            {
                var lineObject = new GameObject("Translator Line");
                var lineRenderer = lineObject.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = false;
                lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 6f });
                lineRenderer.startWidth = 0.005f;
                lineRenderer.endWidth = 0.001f;
                lineRenderer.endColor = Color.clear;
                lineRenderer.startColor = new Color(0.4f, 0.5f, 0.8f, 0.3f); ;
                lineRenderer.material.shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");

                lineObject.transform.SetParent(translator, false);
                lineObject.transform.localPosition = new Vector3(0.74f, 0.37f, 0f);
                lineObject.transform.localRotation = Quaternion.Euler(0f, 353f, 0f);

                lineObject.AddComponent<ConditionalRenderer>().GetShouldRender = () => ToolHelper.Swapper.IsInToolMode(ToolMode.Translator, ToolGroup.Suit);

                nomaiTranslator._raycastTransform = lineObject.transform;

                return lineObject.transform;
            }

            private Transform SetUpTranslatorButtons(Transform translator)
            {
                handheldButtons = new List<TouchButton>(4);
                var buttons = Instantiate(AssetLoader.TranslatorHandheldButtonsPrefab).transform;
                buttons.parent = translator.Find("TranslatorGroup/Props_HEA_Translator");
                buttons.localScale = Vector3.one;
                buttons.localPosition = Vector3.zero;
                buttons.localRotation = Quaternion.identity;

                for (var i = 0; i < buttons.childCount; i++)
                {
                    var touchButton = buttons.GetChild(i).gameObject.AddComponent<TouchButton>();

                    if (touchButton.name == "Up" || touchButton.name == "Down")
                        touchButton.CheckEnabled = () => nomaiTranslator._translatorProp._scrollRect.verticalScrollbar.isActiveAndEnabled;

                    handheldButtons.Add(touchButton);
                }

                return buttons;
            }

            private void RemoveTextMaterials(Transform translator)
            {
                var texts = translator.gameObject.GetComponentsInChildren<Graphic>(true);
                foreach (var text in texts)
                {
                    text.material = null;
                }
            }

            private void SetUpHolster(Transform translatorModel)
            {
                var holsterModel = Instantiate(translatorModel).gameObject;
                holsterModel.SetActive(true);
                var holsterTool = holsterModel.AddComponent<HolsterTool>();
                holsterTool.position = new Vector3(-0.3f, 0, 0);
                holsterTool.mode = ToolMode.Translator;
                holsterTool.scale = 0.15f;
                holsterTool.angle = new Vector3(0, 90, 90);
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ToolModeSwapper>(nameof(ToolModeSwapper.IsTranslatorEquipPromptAllowed), nameof(IsPromptAllowed));
                    Postfix<ToolModeSwapper>(nameof(ToolModeSwapper.GetAutoEquipTranslator), nameof(IsPromptAllowed));
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unusued parameter is needed for return value passthrough.")]
                private static bool IsPromptAllowed(bool __result)
                {
                    return false;
                }
            }
        }
    }
}