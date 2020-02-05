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
                var signalscopeScreen = cockpitUI.Find("SignalScreen").gameObject;

                var collider = probeScreen.AddComponent<SphereCollider>();
                collider.radius = 0.4f;
                collider.isTrigger = true;

                var interaction = probeScreen.AddComponent<InteractReceiver>();
                interaction.SetInteractRange(2);
                interaction.SetValue("_usableInShip", true);
                interaction.SetPromptText(UITextType.Probe_Title);
                interaction.OnPressInteract += OnToolInteract(ToolMode.Probe);

            }

            private static SingleInteractionVolume.PressInteractEvent OnToolInteract (ToolMode mode) {
                var swapper = Locator.GetToolModeSwapper();
                return () => {
                    ControllerInput.SimulateButton(XboxButton.RightBumper);
                    //_cockpit.Invoke("ExitLandingView");
                    //if (!OWInput.IsInputMode(InputMode.ShipCockpit)) {
                    //    return;
                    //}
                    //_cockpit.Invoke("ExitLandingView");
                    //swapper.SetValue("_currentToolGroup", ToolGroup.Ship);
                    //if (swapper.IsInToolMode(mode)) {
                    //    swapper.UnequipTool();
                    //} else {
                    //    ControllerInput.SimulateButton(XboxButton.RightBumper, 1);
                    //    swapper.EquipToolMode(mode);
                    //}
                };
            }
        }
    }
}
