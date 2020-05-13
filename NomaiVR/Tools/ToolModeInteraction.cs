using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    class ToolModeInteraction : MonoBehaviour
    {
        void Awake()
        {
            var proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.onEnter = OnDetectorEnter;
            proximityDetector.onExit = OnDetectorExit;
            proximityDetector.other = Hands.LeftHand;
            proximityDetector.exitThreshold = 0.1f;
        }

        void OnDetectorEnter()
        {
            if (Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Translator))
            {
                var translator = FindObjectOfType<NomaiTranslatorProp>();
                var currentTextId = translator.GetValue<int>("_currentTextID");
                var nomaiText = translator.GetValue<NomaiText>("_nomaiTextComponent");
                var textField = translator.GetValue<Text>("_textField");
                var scrollRect = translator.GetValue<ScrollRect>("_scrollRect");
                var isScrollable = textField.preferredHeight > scrollRect.viewport.sizeDelta.y;
                // Using very low value instead of zero, because the final scroll position never gets to exactly zero.
                var hasMoreToScroll = scrollRect.verticalNormalizedPosition > 0.001f;

                if (!nomaiText)
                {
                    return;
                }

                if (isScrollable && hasMoreToScroll)
                {
                    ControllerInput.SimulateInput(XboxAxis.dPadY, -1);
                    return;
                }

                if (currentTextId == nomaiText.GetNumTextBlocks())
                {
                    translator.SetValue("_currentTextID", 0);
                }
            }

            ControllerInput.SimulateInput(XboxAxis.dPadX, 1);
        }

        void OnDetectorExit()
        {
            ControllerInput.SimulateInput(XboxAxis.dPadX, 0);
            ControllerInput.SimulateInput(XboxAxis.dPadY, 0);
        }
    }
}
