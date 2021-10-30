using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Ship
{
    public class ShipSignalscopeInteract: ShipInteractReceiver
    {
        protected override UITextType Text => UITextType.UISignalscope;
        protected override GameObject ComponentContainer => transform.Find("SignalScopeScreenFrame_geo").gameObject;
        private const InputConsts.InputCommandType changeFrequencyCommand = InputConsts.InputCommandType.TOOL_RIGHT;

        protected override void Initialize()
        {
            var sigScopeDisplay = transform.Find("SigScopeDisplay");

            var canvas = new GameObject().AddComponent<Canvas>();
            canvas.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = ShouldRenderScreenText;

            var canvasTransform = canvas.transform;
            canvasTransform.SetParent(sigScopeDisplay.parent, false);
            canvasTransform.localPosition = sigScopeDisplay.localPosition;
            canvasTransform.localRotation = sigScopeDisplay.localRotation;
            canvasTransform.localScale = sigScopeDisplay.localScale;

            var monitorText = new GameObject().AddComponent<Text>();
            monitorText.text = "<color=grey>SIGNALSCOPE</color>\n\ninteract with screen to activate";
            monitorText.color = new Color(1, 1, 1, 0.1f);
            monitorText.alignment = TextAnchor.MiddleCenter;
            monitorText.fontSize = 8;
            monitorText.font = MonitorPromptFont;

            var monitorTextTransform = monitorText.transform;
            monitorTextTransform.SetParent(canvasTransform, false);
            monitorTextTransform.localScale = Vector3.one * 0.5f;
            
            var signalscopeScreenButtons = Instantiate(AssetLoader.SignalscopeScreenButtonsPrefab, sigScopeDisplay);
            foreach (Transform signalscopeScreenButton in signalscopeScreenButtons.transform)
            {
                signalscopeScreenButton.gameObject.AddComponent<ShipSignalscopeButton>();
            }
        }
        
        protected override void Update()
        {
            base.Update();

            if (!Receiver) return;

            var isUsingSignaslcope = ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Ship);
            
            if (isUsingSignaslcope && Receiver._textID != UITextType.SignalscopeFrequencyPrompt)
            {
                Receiver.SetPromptText(UITextType.SignalscopeFrequencyPrompt);
            }
            else if (!isUsingSignaslcope && Receiver._textID != UITextType.UISignalscope)
            {
                Receiver.SetPromptText(UITextType.UISignalscope);
            }
        }

        protected override void OnPress()
        {
            if (ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Ship))
            {
                ControllerInput.SimulateInput(changeFrequencyCommand, true);
            }
            else
            {
                VRToolSwapper.Equip(ToolMode.SignalScope, null);
            }
        }

        protected override void OnRelease()
        {
            if (ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Ship))
            {
                ControllerInput.SimulateInput(changeFrequencyCommand, false);
            }
        }

        protected override bool ShouldDisable()
        {
            return false;
        }
    }
}