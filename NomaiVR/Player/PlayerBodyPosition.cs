using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class PlayerBodyPosition : NomaiVRModule<PlayerBodyPosition.Behaviour, PlayerBodyPosition.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform _cameraParent;
            private static Transform _playArea;
            private Transform _camera;

            private void Start()
            {
                // This component is messing with our ability to read the VR camera's rotation.
                // Seems to be responsible for controlling the camera rotation with the mouse / joystick.
                var playerCameraController = Camera.main.GetComponent<PlayerCameraController>();
                if (playerCameraController)
                {
                    playerCameraController.enabled = false;
                }

                AdjustPlayerHeadPosition();
                SetupCamera();
            }

            private void AdjustPlayerHeadPosition()
            {
                var playerhead = PlayerHelper.PlayerHead;
                playerhead.localPosition = new Vector3(playerhead.localPosition.x, playerhead.localPosition.y, 0);
            }

            private void SetupCamera()
            {
                // Make an empty parent object for moving the camera around.
                _camera = Camera.main.transform;
                _cameraParent = new GameObject().transform;
                _playArea = new GameObject().transform;
                _playArea.parent = Locator.GetPlayerTransform();
                _playArea.position = PlayerHelper.PlayerHead.position;
                _playArea.rotation = PlayerHelper.PlayerHead.rotation;
                _cameraParent.parent = _playArea;
                _cameraParent.localRotation = Quaternion.identity;
                _camera.parent = _cameraParent;

                var movement = PlayerHelper.PlayerHead.position - _camera.position;
                _cameraParent.position += movement;

            }

            private void MoveCameraToPlayerHead()
            {
                var movement = PlayerHelper.PlayerHead.position - _camera.position;
                _cameraParent.position += movement;
            }

            private void Update()
            {
                var cameraToHead = Vector3.ProjectOnPlane(PlayerHelper.PlayerHead.position - _camera.position, PlayerHelper.PlayerHead.up);

                if (cameraToHead.sqrMagnitude > 0.5f)
                {
                    MoveCameraToPlayerHead();
                }
                if (NomaiVR.Config.debugMode)
                {
                    if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    {
                        _cameraParent.localScale *= 0.9f;
                    }
                    if (Input.GetKeyDown(KeyCode.KeypadMinus))
                    {
                        _cameraParent.localScale /= 0.9f;
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<PlayerCharacterController>("UpdateTurning", typeof(Patch), nameof(Patch.PatchTurning));

                    // Prevent camera from locking on to model ship.
                    var lockOnArgs = new[] { typeof(Transform), typeof(float), typeof(bool), typeof(float) };
                    var lockOnMethod = typeof(PlayerLockOnTargeting).GetMethod("LockOn", lockOnArgs);
                    NomaiVR.Helper.HarmonyHelper.AddPrefix(lockOnMethod, typeof(Patch), nameof(PreLockOn));
                }

                private static bool PreLockOn(Transform targetTransform)
                {
                    if (targetTransform.GetComponent<ModelShipController>() != null)
                    {
                        return false;
                    }
                    return true;
                }

                private static void PatchTurning(PlayerCharacterController __instance)
                {
                    if (OWInput.GetInputMode() != InputMode.Character)
                    {
                        return;
                    }

                    var playerCam = __instance.GetValue<OWCamera>("_playerCam");
                    var transform = __instance.GetValue<Transform>("_transform");

                    var fromTo = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up));

                    var magnitudeUp = 1 - Vector3.ProjectOnPlane(playerCam.transform.up, transform.up).magnitude;
                    var magnitudeForward = 1 - Vector3.ProjectOnPlane(playerCam.transform.up, transform.right).magnitude;
                    var magnitude = magnitudeUp + magnitudeForward;

                    if (magnitude < 0.3f)
                    {
                        return;
                    }

                    var targetRotation = fromTo * transform.rotation;
                    var inverseRotation = Quaternion.Inverse(fromTo) * _playArea.rotation;

                    var maxDegreesDelta = magnitude * 3f;
                    _playArea.rotation = Quaternion.RotateTowards(_playArea.rotation, inverseRotation, maxDegreesDelta);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
                }
            }

        }
    }
}
