using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    class ShipTools: MonoBehaviour {
        void Awake () {
            NomaiVR.Log("Start Ship Tools");

            NomaiVR.Helper.HarmonyHelper.AddPostfix<ShipBody>("Start", typeof(Patches), "ShipStart");
        }

        internal static class Patches {
            static void ShipStart (ShipBody __instance) {
                foreach (Transform child in __instance.transform) {
                    NomaiVR.Log("Patch ship", child.gameObject.name);
                }
                var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");
                NomaiVR.Log("cockpitUI ", cockpitUI.name);
                var probeScreen = cockpitUI.Find("ProbeScreen").gameObject;
                NomaiVR.Log("probeScreen ", probeScreen.name);
                var signalscopeScreen = cockpitUI.Find("SignalScreen").gameObject;
                NomaiVR.Log("signalscopeScreen ", signalscopeScreen.name);

                var collider = probeScreen.AddComponent<SphereCollider>();
                collider.radius = 0.2f;
                collider.isTrigger = true;

                var interaction = probeScreen.AddComponent<InteractReceiver>();
                NomaiVR.Log("interaction ", interaction.name);
                interaction.SetInteractRange(2);
                NomaiVR.Log("_usableInShip before");

                interaction.SetValue("_usableInShip", true);
                NomaiVR.Log("_usableInShip after");
                interaction.SetPromptText(UITextType.Probe_Title);

                NomaiVR.Log("after interaction ");


            }
        }
    }
}
