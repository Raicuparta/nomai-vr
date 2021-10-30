using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Tools;
using UnityEngine;
using UnityEngine.UI;
using static InputConsts;

namespace NomaiVR.Ship
{
    public class ShipSignalscopeInteract: ShipInteractReceiver
    {
        protected override UITextType Text => UITextType.UISignalscope;
        protected override GameObject ComponentContainer => transform.Find("SignalScopeScreenFrame_geo").gameObject;

        protected override void Awake()
        {
            base.Awake();
            Receiver.SetPromptText(UITextType.UISignalscope);
            
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

        protected override void OnPress()
        {
           VRToolSwapper.Equip(ToolMode.SignalScope, null);
        }

        protected override void OnRelease()
        {
        }

        protected override bool ShouldDisable()
        {
            return ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Ship);
        }
    }
}