using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class FixProbeCannonVisibility: NomaiVRModule<NomaiVRModule.EmptyBehaviour, FixProbeCannonVisibility.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        internal class Patch: NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<AstroObject>(nameof(AstroObject.Awake), nameof(RotateTimberHearth));
                Postfix<PlayerSpawner>(nameof(PlayerSpawner.SpawnPlayer), nameof(RotatePlayer));
            }
            
            private static void RotateTimberHearth(AstroObject __instance)
            {
                if (__instance._name != AstroObject.Name.TimberHearth) return;

                __instance.transform.eulerAngles += new Vector3(37, 18, 0);
            }

            private static void RotatePlayer(PlayerSpawner __instance)
            {
                if (__instance._initialSpawnPoint == null || LoadManager.GetCurrentScene() != OWScene.SolarSystem) return;
                var playerTransform = __instance._playerBody.transform;
		        var playerPosition = playerTransform.position;
                var playerUp = playerTransform.up;
		        var projectedPosition = Vector3.Project(playerPosition, playerUp) - playerPosition;
		        var rotationOffset = Quaternion.AngleAxis(Vector3.Angle(playerTransform.forward, projectedPosition) * 40, playerUp);
		        playerTransform.rotation = rotationOffset * playerTransform.rotation;
                
            }
        }
    }
}