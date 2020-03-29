using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    class ToolModeInteraction: MonoBehaviour {
        void Awake () {
            var proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.onEnter = OnDetectorEnter;
            proximityDetector.onExit = OnDetectorExit;
            proximityDetector.other = Hands.LeftHand;
            proximityDetector.exitThreshold = 0.1f;
        }

        void OnDetectorEnter () {
            if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Translator)) {
                var translator = FindObjectOfType<NomaiTranslatorProp>();
                var currentTextId = translator.GetValue<int>("_currentTextID");
                var nomaiText = translator.GetValue<NomaiText>("_nomaiTextComponent");

                if (!nomaiText) {
                    return;
                }

                if (currentTextId == nomaiText.GetNumTextBlocks()) {
                    translator.SetValue("_currentTextID", 0);
                }
            }

            ControllerInput.SimulateInput(XboxAxis.dPadX, 1);
        }

        void OnDetectorExit () {
            ControllerInput.SimulateInput(XboxAxis.dPadX, 0);
        }
    }
}
