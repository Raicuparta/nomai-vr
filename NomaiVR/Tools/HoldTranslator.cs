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
            private LineRenderer _lineRenderer;

            internal void Start()
            {
                var translator = Camera.main.transform.Find("NomaiTranslatorProp");

                var holdTranslator = translator.gameObject.AddComponent<Holdable>();
                holdTranslator.transform.localPosition = new Vector3(-0.2f, 0.107f, 0.02f);
                holdTranslator.transform.localRotation = Quaternion.Euler(32.8f, 0f, 0f);

                var translatorGroup = translator.Find("TranslatorGroup");
                translatorGroup.localPosition = Vector3.zero;
                translatorGroup.localRotation = Quaternion.identity;

                translator.localScale = Vector3.one * 0.3f;
                var translatorModel = translatorGroup.Find("Props_HEA_Translator");
                translatorModel.localPosition = Vector3.zero;
                translatorModel.localRotation = Quaternion.identity;

                var laser = SetUpLaser(translator);

                translator.GetComponent<NomaiTranslator>().SetValue("_raycastTransform", laser);

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

                var texts = translator.gameObject.GetComponentsInChildren<Graphic>(true);

                foreach (var text in texts)
                {
                    text.material = null;
                }

                var translatorHolster = Instantiate(translatorModel).gameObject;
                translatorHolster.SetActive(true);
                var holster = translatorHolster.AddComponent<HolsterTool>();
                holster.hand = HandsController.Behaviour.RightHand;
                holster.position = new Vector3(-0.3f, 0, 0);
                holster.mode = ToolMode.Translator;
                holster.scale = 0.15f;
                holster.angle = new Vector3(0, 90, 90);

                translatorGroup.Find("TranslatorBeams").localScale = Vector3.one / 0.3f;

                SetUpLaser(translatorModel);
            }

            private Transform SetUpLaser(Transform translator)
            {
                Logs.Write("Setting up laser");
                var lineObject = new GameObject("Translator Line");
                _lineRenderer = lineObject.AddComponent<LineRenderer>();
                _lineRenderer.useWorldSpace = false;
                _lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 6f });
                _lineRenderer.startWidth = 0.005f;
                _lineRenderer.endWidth = 0.001f;
                _lineRenderer.endColor = Color.clear;
                _lineRenderer.startColor = new Color(1, 1, 1, 0.3f); ;
                _lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");

                lineObject.transform.SetParent(translator, false);
                lineObject.transform.localPosition = new Vector3(0.74f, 0.37f, 0f);
                lineObject.transform.localRotation = Quaternion.Euler(0f, 353f, 0f);

                return lineObject.transform;

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