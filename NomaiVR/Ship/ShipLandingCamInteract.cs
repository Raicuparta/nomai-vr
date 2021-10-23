using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Ship
{
    public class ShipLandingCamInteract: ShipInteractReceiver
    {
        protected override UITextType Text => UITextType.ShipLandingPrompt;
        protected override GameObject ComponentContainer => gameObject;
        private const InputConsts.InputCommandType inputCommandType = InputConsts.InputCommandType.LANDING_CAMERA;
        private static ShipCockpitController cockpitController;

        protected override void Initialize()
        {
            cockpitController = FindObjectOfType<ShipCockpitController>();
                
            var canvas = new GameObject().AddComponent<Canvas>();
            canvas.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = () => ShouldRenderScreenText() && !cockpitController._landingCam.enabled;

            var canvasTransform = canvas.transform;
            canvasTransform.SetParent(transform.parent, false);
            canvasTransform.localPosition = new Vector3(-0.017f, 3.731f, 5.219f);
            canvasTransform.localRotation = Quaternion.Euler(53.28f, 0, 0);
            canvasTransform.localScale = Vector3.one * 0.007f;
            
            var landingText = new GameObject().AddComponent<Text>();
            landingText.text = "<color=grey>LANDING CAMERA</color>\n\ninteract with screen\nto activate";
            landingText.color = new Color(1, 1, 1, 0.1f);
            landingText.alignment = TextAnchor.MiddleCenter;
            landingText.fontSize = 8;
            landingText.font = MonitorPromptFont;
            
            var landingTextTransform = landingText.transform;
            landingTextTransform.SetParent(canvas.transform, false);
            landingTextTransform.localScale = Vector3.one * 0.6f;
        }

        protected override void OnPress()
        {
            if (cockpitController._landingCam.enabled)
            {
                cockpitController.ExitLandingView();
            }
            else
            {
                ControllerInput.SimulateInput(inputCommandType, true);
            }
        }

        protected override void OnRelease()
        {
            ControllerInput.SimulateInput(inputCommandType, false);
        }

        protected override bool ShouldDisable()
        {
            return  ToolHelper.Swapper.IsInToolMode(ToolMode.Probe, ToolGroup.Ship);
        }
    }
}