using OWML.Common;
using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class HoldSignalscope : MonoBehaviour
    {
        protected static Transform SignalscopeReticule;
        protected static Transform ShipWindshield;
        protected static Signalscope SignalScope;

        void Awake() {
            // For fixing signalscope zoom
            //NomaiVR.Helper.HarmonyHelper.AddPostfix<Signalscope>("EnterSignalscopeZoom", typeof(Patches), "ZoomIn");
            //NomaiVR.Helper.HarmonyHelper.AddPostfix<Signalscope>("ExitSignalscopeZoom", typeof(Patches), "ZoomOut");
            //behaviour.SetValue("_targetFOV", Common.MainCamera.fieldOfView);

            NomaiVR.Helper.HarmonyHelper.AddPrefix<OWInput>("ChangeInputMode", typeof(Patches), "ChangeInputMode");
            NomaiVR.Helper.Events.Subscribe<ShipCockpitUI>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;

            ShipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;


            var signalScope = Common.MainCamera.transform.Find("Signalscope");
            MotionControls.HoldObject(signalScope, MotionControls.RightHand, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));
            SignalScope = signalScope.GetComponent<Signalscope>();

            var signalScopeModel = signalScope.GetChild(0);
            // Tools have a special shader that draws them on top of everything
            // and screws with perspective. Changing to Standard shader so they look
            // like a normal 3D object.
            signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            signalScopeModel.localPosition = Vector3.up * -0.1f;
            signalScopeModel.localRotation = Quaternion.identity;

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it looks glitchy and doesn't seem necessary.
            signalScopeModel.GetChild(0).gameObject.SetActive(false);

            var signalScopeHolster = Instantiate(signalScopeModel).gameObject;
            signalScopeHolster.SetActive(true);
            var holster = signalScopeHolster.AddComponent<HolsterTool>();
            holster.hand = MotionControls.RightHand;
            holster.position = new Vector3(0.3f, 0.35f, 0);
            holster.mode = ToolMode.SignalScope;
            holster.scale = 0.8f;
            holster.angle = Vector3.right * 90;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            SignalscopeReticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");
            var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
            var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");

            // Attatch Signalscope UI to the Signalscope.
            SignalscopeReticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            SignalscopeReticule.parent = signalScope;
            SignalscopeReticule.localScale = Vector3.one * 0.0005f;
            SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
            SignalscopeReticule.localRotation = Quaternion.identity;

            helmetOff.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOff.parent = signalScope;
            helmetOff.localScale = Vector3.one * 0.0005f;
            helmetOff.localPosition = Vector3.zero;
            helmetOff.localRotation = Quaternion.identity;

            helmetOn.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOn.parent = signalScope;
            helmetOn.localScale = Vector3.one * 0.0005f;
            helmetOn.localPosition = Vector3.down * 0.5f;
            helmetOn.localRotation = Quaternion.identity;
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(ShipCockpitUI) && ev == Events.AfterStart) {
                behaviour.SetValue("_signalscopeTool", SignalScope);
            }
        }

        internal static class Patches
        {
            static void ZoomIn() {
                Common.MainCamera.transform.localScale = Vector3.one * 0.1f;
            }

            static void ZoomOut() {
                Common.MainCamera.transform.localScale = Vector3.one;
            }

            static void ChangeInputMode(InputMode mode) {
                if (!SignalscopeReticule) {
                    return;
                }
                if (mode == InputMode.ShipCockpit || mode == InputMode.LandingCam) {
                    SignalscopeReticule.parent = ShipWindshield;
                    SignalscopeReticule.localScale = Vector3.one * 0.004f;
                    SignalscopeReticule.localPosition = Vector3.forward * 3f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                } else {
                    SignalscopeReticule.parent = SignalScope.transform;
                    SignalscopeReticule.localScale = Vector3.one * 0.0005f;
                    SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
