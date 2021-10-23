using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Ship
{
    public class ShipSignalscopeInteract: ShipInteractReceiver
    {
        protected override UITextType Text => UITextType.UISignalscope;
        protected override string ChildName => "SignalScopeScreenFrame_geo";

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
        }

        protected override void OnPress()
        {
            VRToolSwapper.Equip(ToolMode.SignalScope, null);
        }

        protected override void OnRelease()
        {
        }

        protected override bool IsUsingTool()
        {
            return  ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Ship);
        }
    }
}