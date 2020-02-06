using UnityEngine;

namespace NomaiVR {
    class ShipTools: MonoBehaviour {
        static ShipCockpitController _cockpit;
        void Awake () {
            NomaiVR.Log("Start Ship Tools");

            _cockpit = FindObjectOfType<ShipCockpitController>();
            NomaiVR.Helper.HarmonyHelper.AddPostfix<ShipBody>("Start", typeof(Patches), "ShipStart");
        }

        internal static class Patches {
            static void ShipStart (ShipBody __instance) {
                var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");

                var probe = cockpitUI.Find("ProbeScreen/ProbeScreenPivot/ProbeScreen").gameObject.AddComponent<ButtonInteraction>();
                probe.button = XboxButton.RightBumper;
                probe.text = UITextType.ScoutModePrompt;


                var signalscope = cockpitUI.Find("SignalScreen/SignalScreenPivot/SignalScopeScreenFrame_geo").gameObject.AddComponent<ButtonInteraction>();
                signalscope.button = XboxButton.Y;
                signalscope.text = UITextType.SignalscopePrompt;

                var landingCam = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior/LandingCamScreen").gameObject.AddComponent<ButtonInteraction>();
                landingCam.button = XboxButton.DPadDown;
                landingCam.text = UITextType.ShipLandingPrompt;
            }
        }
    }
}
