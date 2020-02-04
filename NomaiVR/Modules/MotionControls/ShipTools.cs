using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    class ShipTools: MonoBehaviour {
        static ProbeLauncher _shipProbeLauncher;
        void Awake () {
            NomaiVR.Log("Start Ship Tools");

            _shipProbeLauncher = GameObject.FindObjectOfType<ShipCockpitController>().GetShipProbeLauncher();
            _shipProbeLauncher.EquipTool();
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
                interaction.OnPressInteract += OnProbeInteract;

            }

            private static void OnProbeInteract () {
                var activeProbe = _shipProbeLauncher.GetActiveProbe();
                if (activeProbe == null) {
                    _shipProbeLauncher.Invoke("LaunchProbe");
                } else {
                    _shipProbeLauncher.Invoke("TakeSnapshotWithCamera", activeProbe.GetForwardCamera());
                }
            }
        }
    }
}
