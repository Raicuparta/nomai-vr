namespace NomaiVR.EffectFixes
{
    internal class DisableDeathAnimation : NomaiVRModule<NomaiVRModule.EmptyBehaviour, DisableDeathAnimation.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<PlayerCharacterController>(nameof(PlayerCharacterController.OnPlayerDeath), nameof(PrePlayerDeath));
            }

            private static bool PrePlayerDeath(DeathType deathType)
            {
                if (deathType == DeathType.Impact || deathType == DeathType.Default || deathType == DeathType.Asphyxiation)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
