using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class ToolModeInteraction : MonoBehaviour
    {
        internal void Start()
        {
            var proximityDetector = gameObject.AddComponent<ProximityDetector>();
            proximityDetector.onEnter = OnDetectorEnter;
            proximityDetector.onExit = OnDetectorExit;
            proximityDetector.other = HandsController.Behaviour.LeftHand;
            proximityDetector.exitThreshold = 0.1f;
        }

        internal void Update()
        {
            if (OWInput.IsNewlyReleased(InputLibrary.interact))
            {
                OnDetectorEnter();
            }
        }

        private void OnDetectorEnter()
        {
            if (ToolHelper.Swapper.IsInToolMode(ToolMode.Translator))
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
                    ControllerInput.Behaviour.SimulateInput(AxisIdentifier.CTRLR_DPADY, -1);
                    return;
                }

                if (currentTextId == nomaiText.GetNumTextBlocks())
                {
                    translator.SetValue("_currentTextID", 0);
                }
            }

            ControllerInput.Behaviour.SimulateInput(AxisIdentifier.CTRLR_DPADX, 1);
        }

        private void OnDetectorExit()
        {
            ControllerInput.Behaviour.SimulateInput(AxisIdentifier.CTRLR_DPADX, 0);
            ControllerInput.Behaviour.SimulateInput(AxisIdentifier.CTRLR_DPADY, 0);
        }
    }
}
