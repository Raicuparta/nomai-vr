using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR {
    public class HoldTranslator: MonoBehaviour {
        void Awake () {
            var translator = Camera.main.transform.Find("NomaiTranslatorProp");

            Hands.HoldObject(translator, Hands.RightHand, new Vector3(-0.2f, 0.107f, 0.02f), Quaternion.Euler(32.8f, 0f, 0f));

            var translatorGroup = translator.Find("TranslatorGroup");
            translatorGroup.localPosition = Vector3.zero;
            translatorGroup.localRotation = Quaternion.identity;

            translator.localScale = Vector3.one * 0.3f;
            var translatorModel = translatorGroup.Find("Props_HEA_Translator");
            translatorModel.localPosition = Vector3.zero;
            translatorModel.localRotation = Quaternion.identity;

            translator.GetComponent<NomaiTranslator>().SetValue("_raycastTransform", translatorModel);

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it looks glitchy and doesn't seem necessary.
            translatorModel.Find("Props_HEA_Translator_Prepass").gameObject.SetActive(false);

            var renderers = translatorModel.gameObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var renderer in renderers) {
                foreach (var material in renderer.materials) {
                    material.shader = Shader.Find("Standard");
                }
            }

            var texts = translator.gameObject.GetComponentsInChildren<Graphic>(true);

            foreach (var text in texts) {
                text.material = null;
            }

            var translatorHolster = Instantiate(translatorModel).gameObject;
            translatorHolster.SetActive(true);
            var holster = translatorHolster.AddComponent<HolsterTool>();
            holster.hand = Hands.RightHand;
            holster.position = new Vector3(-0.3f, -0.55f, 0);
            holster.mode = ToolMode.Translator;
            holster.scale = 0.15f;
            holster.angle = new Vector3(0, 90, 90);

            translatorGroup.Find("TranslatorBeams").localScale = Vector3.one / 0.3f;

            translator.gameObject.AddComponent<ToolModeInteraction>();
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ToolModeSwapper>("IsTranslatorEquipPromptAllowed", typeof(Patches), nameof(IsPromptAllowed));
                NomaiVR.Post<ToolModeSwapper>("GetAutoEquipTranslator", typeof(Patches), nameof(IsPromptAllowed));
                NomaiVR.Post<ToolModeSwapper>("IsNomaiTextInFocus", typeof(Patches), nameof(IsPromptAllowed));
            }

            static bool IsPromptAllowed (bool __result) {
                return false;
            }
        }
    }
}
