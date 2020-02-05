using OWML.ModHelper.Events;
using System;
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

                var probeScreen = cockpitUI.Find("ProbeScreen/ProbeScreenPivot/ProbeCamDisplay").gameObject;
                probeScreen.AddComponent<ButtonInteraction>().button = XboxButton.RightBumper;

                var signalscopeScreen = cockpitUI.Find("SignalScreen/SignalScreenPivot/StaticAudioSource").gameObject;
                signalscopeScreen.AddComponent<ButtonInteraction>().button = XboxButton.Y;
            }
        }
    }
}
