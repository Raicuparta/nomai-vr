using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    internal class PlayerBodyPosition : NomaiVRModule<PlayerBodyPosition.Behaviour, PlayerBodyPosition.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform _cameraParent;
            private static Transform _playArea;
            private Transform _camera;
            private static Animator _playerAnimator;

            internal void Start()
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
                CreateRecenterMenuEntry();
                _playerAnimator = FindObjectOfType<PlayerAnimController>().GetValue<Animator>("_animator");
            }

            private static void AdjustPlayerHeadPosition()
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

            private void CreateRecenterMenuEntry()
            {
                var button = NomaiVR.Helper.Menus.PauseMenu.OptionsButton.Duplicate("RESET VR POSITION");
                button.OnClick += MoveCameraToPlayerHead;
            }

            internal void Update()
            {
                var cameraToHead = Vector3.ProjectOnPlane(PlayerHelper.PlayerHead.position - _camera.position, PlayerHelper.PlayerHead.up);

                if (cameraToHead.sqrMagnitude > 0.5f)
                {
                    MoveCameraToPlayerHead();
                }
                if (ModSettings.DebugMode)
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
                    Postfix<PlayerCharacterController>("UpdateTurning", nameof(Patch.PatchTurning));
                }

                private static void PatchTurning(OWCamera ____playerCam, Transform ____transform)
                {
                    var runSpeedX = _playerAnimator.GetFloat("RunSpeedX");
                    var runSpeedY = _playerAnimator.GetFloat("RunSpeedY");
                    var isStopped = runSpeedX + runSpeedY == 0;
                    var isControllerOriented = !isStopped && ModSettings.ControllerOrientedMovement;

                    if ((OWInput.GetInputMode() != InputMode.Character))
                    {
                        return;
                    }

                    var rotationSource = isControllerOriented ? LaserPointer.Behaviour.LeftHandLaser : ____playerCam.transform;

                    var fromTo = Quaternion.FromToRotation(____transform.forward, Vector3.ProjectOnPlane(rotationSource.transform.forward, ____transform.up));

                    var magnitude = 0f;
                    if (!isControllerOriented)
                    {
                        var magnitudeUp = 1 - Vector3.ProjectOnPlane(rotationSource.transform.up, ____transform.up).magnitude;
                        var magnitudeForward = 1 - Vector3.ProjectOnPlane(rotationSource.transform.up, ____transform.right).magnitude;
                        magnitude = magnitudeUp + magnitudeForward;

                        if (magnitude < 0.3f)
                        {
                            return;
                        }
                    }

                    var targetRotation = fromTo * ____transform.rotation;
                    var inverseRotation = Quaternion.Inverse(fromTo) * _playArea.rotation;

                    if (isControllerOriented)
                    {
                        _playArea.rotation = inverseRotation;
                        ____transform.rotation = targetRotation;
                    }
                    else
                    {
                        var maxDegreesDelta = magnitude * 3f;
                        _playArea.rotation = Quaternion.RotateTowards(_playArea.rotation, inverseRotation, maxDegreesDelta);
                        ____transform.rotation = Quaternion.RotateTowards(____transform.rotation, targetRotation, maxDegreesDelta);
                    }


                }
            }

        }
    }
}
