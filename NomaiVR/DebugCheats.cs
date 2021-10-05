using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class DebugCheats: NomaiVRModule<DebugCheats.Behaviour, DebugCheats.Patch>
    {
        public class Behaviour : MonoBehaviour
        {
            private void Update()
            {
                if (!SteamVR_Actions.default_Jump.state || !SteamVR_Actions.default_Back.state) return;

                // Wake up in dream (requires invincibility).
                if (SteamVR_Actions.default_Interact.stateDown)
                {
                    var dreamWorldController = FindObjectOfType<DreamWorldController>();
                    dreamWorldController._dreamCampfire = FindObjectOfType<DreamCampfire>();
                    dreamWorldController._dreamArrivalPoint = FindObjectOfType<DreamArrivalPoint>();
                    dreamWorldController._relativeSleepLocation = new RelativeLocationData(new Vector3(0f, 1f, -2f), Quaternion.identity, Vector3.zero);
                    dreamWorldController._playerLantern = FindObjectOfType<DreamLanternItem>();
                    dreamWorldController._enteringDream = true;
                    FindObjectOfType<ItemTool>().MoveItemToCarrySocket(FindObjectOfType<DreamLanternItem>());
                }
                
                // Start mind projection.
                if (SteamVR_Actions.default_Grip.stateDown)
                {
                    FindObjectOfType<MindSlideProjector>().Play(true);
                }
            }
        }
        
        internal class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<DebugInputManager>(nameof(DebugInputManager.Update), nameof(ForceDebugInputMode));
                Postfix<EntitlementsManager>(nameof(EntitlementsManager.IsDlcOwned), nameof(GimmeFreeDlc));
                Prefix<DebugInputManager>(nameof(DebugInputManager.Awake), nameof(ForceDebugMode));
                Empty<DebugInputManager>(nameof(DebugInputManager.CheckDebugInputMode));
            }

            private static bool ForceDebugMode(DebugInputManager __instance)
            {
                __instance._debugInputMode = DebugInputManager.DebugInputMode.NORMAL;
                
                // Prevent behaviour from destroying itself in Awake.
                return false;
            }

            private static void ForceDebugInputMode(DebugInputManager __instance)
            {
                __instance._debugInputMode = DebugInputManager.DebugInputMode.NORMAL;
            }

            private static void GimmeFreeDlc(ref EntitlementsManager.AsyncOwnershipStatus __result)
            {
                __result = EntitlementsManager.AsyncOwnershipStatus.Owned;
            }
        }

        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => PlayableScenes;
    }
}