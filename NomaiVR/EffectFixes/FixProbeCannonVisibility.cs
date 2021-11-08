using UnityEngine;

namespace NomaiVR.EffectFixes
{
    // Seeing the Probe Cannon explosion when waking up from each loop is a pretty important detail in the game.
    // In VR, the player no longer wakes up facing the sky, so this detail is much easier to miss.
    // By rotating TH, we can make GD show at an angle that's visible while looking straight ahead.
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

                var shipTransform = GameObject.FindWithTag("Ship").transform;
                var shipParent = shipTransform.parent;
                shipTransform.SetParent(__instance.transform);
                __instance.transform.eulerAngles += new Vector3(37, 18, 0);
                shipTransform.SetParent(shipParent);
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