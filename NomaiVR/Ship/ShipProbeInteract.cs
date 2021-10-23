using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Ship
{
    public class ShipProbeInteract: ShipInteractReceiver
    {
        protected override UITextType Text => UITextType.ScoutModePrompt;
        protected override GameObject ComponentContainer => transform.Find("ProbeScreen").gameObject;

        protected override void Initialize()
        {
            var probeCamDisplay = transform.Find("ProbeCamDisplay");

            var monitorText = new GameObject().AddComponent<Text>();
            monitorText.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = ShouldRenderScreenText;
            monitorText.text = "<color=grey>PROBE LAUNCHER</color>\n\ninteract with screen\nto activate";
            monitorText.color = new Color(1, 1, 1, 0.1f);
            monitorText.alignment = TextAnchor.MiddleCenter;
            monitorText.fontSize = 8;
            monitorText.font = MonitorPromptFont;

            var monitorTextTransform = monitorText.transform;
            monitorTextTransform.SetParent(probeCamDisplay.transform, false);
            monitorTextTransform.localScale = Vector3.one * 0.0035f;
            monitorTextTransform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        protected override void OnPress()
        {
            VRToolSwapper.Equip(ToolMode.Probe, null);
        }

        protected override void OnRelease()
        {
        }

        protected override bool IsUsingTool()
        {
            return  ToolHelper.Swapper.IsInToolMode(ToolMode.Probe, ToolGroup.Ship);
        }
    }
}