using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class HoldTranslator : NomaiVRModule<HoldTranslator.Behaviour, HoldTranslator.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {
                var translator = SetUpTranslator();
                SetUpHoldable(translator);
                var translatorGroup = SetUpTranslatorGroup(translator);
                var translatorModel = SetUpTranslatorModel(translatorGroup);
                SetUpLaser(translator);
                RemoveTextMaterials(translator);
                SetUpHolster(translatorModel);
                SetUpLaser(translator);
            }

            private Transform SetUpTranslator()
            {
                var translator = Locator.GetPlayerCamera().transform.Find("NomaiTranslatorProp");
                translator.localScale = Vector3.one * 0.3f;
                return translator;
            }

            private void SetUpHoldable(Transform translator)
            {
                var holdTranslator = translator.gameObject.AddComponent<Holdable>();
                holdTranslator.transform.localPosition = new Vector3(-0.2f, 0.107f, 0.02f);
                holdTranslator.transform.localRotation = Quaternion.Euler(32.8f, 0f, 0f);
            }

            private Transform SetUpTranslatorGroup(Transform translator)
            {
                var translatorGroup = translator.Find("TranslatorGroup");
                translatorGroup.localPosition = Vector3.zero;
                translatorGroup.localRotation = Quaternion.identity;
                translatorGroup.Find("TranslatorBeams").localScale = Vector3.one / 0.3f;
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
                lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");

                lineObject.transform.SetParent(translator, false);
                lineObject.transform.localPosition = new Vector3(0.74f, 0.37f, 0f);
                lineObject.transform.localRotation = Quaternion.Euler(0f, 353f, 0f);

                lineObject.AddComponent<ConditionalRenderer>().getShouldRender = () => ToolHelper.Swapper.IsInToolMode(ToolMode.Translator, ToolGroup.Suit);

                translator.GetComponent<NomaiTranslator>().SetValue("_raycastTransform", lineObject.transform);

                return lineObject.transform;
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
                holsterTool.hand = HandsController.Behaviour.RightHand;
                holsterTool.position = new Vector3(-0.3f, 0, 0);
                holsterTool.mode = ToolMode.Translator;
                holsterTool.scale = 0.15f;
                holsterTool.angle = new Vector3(0, 90, 90);
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ToolModeSwapper>("IsTranslatorEquipPromptAllowed", nameof(IsPromptAllowed));
                    Postfix<ToolModeSwapper>("GetAutoEquipTranslator", nameof(IsPromptAllowed));
                    Postfix<ToolModeSwapper>("IsNomaiTextInFocus", nameof(IsPromptAllowed));
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