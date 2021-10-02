namespace NomaiVR
{
    internal class DebugCheats: NomaiVRModule<NomaiVRModule.EmptyBehaviour, DebugCheats.Patch>
    {
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