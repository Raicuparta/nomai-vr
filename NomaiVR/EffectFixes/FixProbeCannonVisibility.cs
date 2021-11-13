using System.Linq;
using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.EffectFixes
{
    // Seeing the Probe Cannon explosion when waking up from each loop is a pretty important detail in the game.
    // In VR, the player no longer wakes up facing the sky, so this detail is much easier to miss.
    // By rotating TH, we can make GD show at an angle that's visible while looking straight ahead.
    internal class FixProbeCannonVisibility: NomaiVRModule<FixProbeCannonVisibility.Behaviour, FixProbeCannonVisibility.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        internal class Behaviour : MonoBehaviour
        {
            internal class Patch: NomaiVRPatch
            {
                private static bool isInitialized;
            
                public override void ApplyPatches()
                {
                    // OWRigidBody will unparent every physics object, so the rotation of Timber Hearth needs to happen
                    // before OWRigidBody.Awake. Otherwise those objects would stay behind.
                    Prefix<OWRigidbody>(nameof(OWRigidbody.Awake), nameof(RotateTimberHearth));
                    Postfix<PlayerSpawner>(nameof(PlayerSpawner.SpawnPlayer), nameof(RotatePlayer));
                    
                    LoadManager.OnStartSceneLoad += (scene, loadScene) => isInitialized = false;
                }
                
                // Rotate Timber Hearth so that Giand's Deep is visible without the player looking up.
                private static void RotateTimberHearth()
                {
                    if (isInitialized) return;
                    isInitialized = true;
                    
                    if (!SceneHelper.IsInSolarSystem()) return;

                    var timberHearth = FindObjectsOfType<AstroObject>().First(astroObject =>
                        astroObject.GetAstroObjectName() == AstroObject.Name.TimberHearth);
                    if (!timberHearth) return;

                    timberHearth.transform.eulerAngles += new Vector3(37, 18, 0);
                }

                // Rotate player around their Y axis so that they start facing Giant's Deep.
                private static void RotatePlayer(PlayerSpawner __instance)
                {
                    if (__instance._initialSpawnPoint == null || LoadManager.GetCurrentScene() != OWScene.SolarSystem) return;
                    var playerTransform = __instance._playerBody.transform;
		            var playerPosition = playerTransform.position;
                    var playerUp = playerTransform.up;
                    var playerForward = playerTransform.forward;
		            var projectedPosition = Vector3.Project(playerPosition, playerUp) - playerPosition;
                    var giantsDeepPosition = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).transform.position;
                    var projectedForward = Vector3.Project(playerForward, playerUp);
                    var siantsDeepDirection = Vector3.Project(giantsDeepPosition - playerPosition, playerUp);
                    var angle = Vector3.Angle(projectedForward, siantsDeepDirection);
		            var rotationOffset = Quaternion.AngleAxis(Vector3.Angle(playerForward, projectedPosition) + angle, playerUp);
		            playerTransform.rotation = rotationOffset * playerTransform.rotation;
                }
            }
        }
        
    }
}