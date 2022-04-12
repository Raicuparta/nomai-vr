using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.Input;
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

        protected override void Awake()
        {
            base.Awake();
            var probeCamDisplay = transform.Find("ProbeCamDisplay");

            var monitorText = new GameObject("VrShipProbeMonitorText").AddComponent<Text>();
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

            var probeScreenButtons = Instantiate(AssetLoader.ProbeScreenButtonsPrefab, probeCamDisplay);
            foreach (Transform probeScreenButton in probeScreenButtons.transform)
            {
                probeScreenButton.gameObject.AddComponent<ShipProbeButton>();
            }
        }

        protected override void OnPress()
        {
            ControllerInput.SimulateInput(InputConsts.InputCommandType.TOOL_PRIMARY, true);
        }

        protected override void OnRelease()
        {
            ControllerInput.SimulateInput(InputConsts.InputCommandType.TOOL_PRIMARY, false);
        }

        protected override bool ShouldDisable()
        {
            return ToolHelper.Swapper.IsInToolMode(ToolMode.Probe, ToolGroup.Ship);
        }
    }
}
