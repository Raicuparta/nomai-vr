using OWML.Utils;
using System;
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
            private static OWCamera _playerCamera;
            private static Animator _playerAnimator;
            private static OWRigidbody _playerBody;
            private static PlayerCharacterController _playerController;

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
                _playerBody = Locator.GetPlayerBody();
                _playerController = _playerBody.GetComponent<PlayerCharacterController>();
            }

            private static void AdjustPlayerHeadPosition()
            {
                var playerhead = PlayerHelper.PlayerHead;
                playerhead.localPosition = new Vector3(playerhead.localPosition.x, playerhead.localPosition.y, 0);
            }

            private void SetupCamera()
            {
                // Make an empty parent object for moving the camera around.
                _playerCamera = Locator.GetPlayerCamera();
                _cameraParent = new GameObject().transform;
                _playArea = new GameObject().transform;
                _playArea.parent = Locator.GetPlayerTransform();
                _playArea.position = PlayerHelper.PlayerHead.position;
                _playArea.rotation = PlayerHelper.PlayerHead.rotation;
                _cameraParent.parent = _playArea;
                _cameraParent.localRotation = Quaternion.identity;
                _playerCamera.transform.parent = _cameraParent;

                var movement = PlayerHelper.PlayerHead.position - _playerCamera.transform.position;
                _cameraParent.position += movement;

            }

            private void MoveCameraToPlayerHead()
            {
                var movement = PlayerHelper.PlayerHead.position - _playerCamera.transform.position;
                _cameraParent.position += movement;
            }

            private void CreateRecenterMenuEntry()
            {
                var button = NomaiVR.Helper.Menus.PauseMenu.OptionsButton.Duplicate("RESET VR POSITION");
                button.OnClick += MoveCameraToPlayerHead;
            }

            internal void Update()
            {
                var cameraToHead = Vector3.ProjectOnPlane(PlayerHelper.PlayerHead.position - _playerCamera.transform.position, PlayerHelper.PlayerHead.up);

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
                    Postfix<PlayerCharacterController>("UpdateTurning", nameof(Patch.PostCharacterTurning));
                    Postfix<JetpackThrusterController>("FixedUpdate", nameof(Patch.PostThrusterUpdate));
                }

                private static void PostThrusterUpdate(Vector3 ____rotationalInput)
                {
                    if (!PlayerState.InZeroG() || ____rotationalInput.sqrMagnitude != 0)
                    {
                        return;
                    }

                    _playerBody.SetAngularVelocity(Vector3.zero);

                    PatchTurning(rotation => _playerBody.MoveToRotation(rotation));
                }

                private static void PostCharacterTurning()
                {
                    PatchTurning(rotation => _playerBody.transform.rotation = rotation);
                }

                private static void PatchTurning(Action<Quaternion> rotationSetter)
                {
                    var runSpeedX = _playerAnimator.GetFloat("RunSpeedX");
                    var runSpeedY = _playerAnimator.GetFloat("RunSpeedY");
                    var isStoppedOnGround = _playerController.IsGrounded() && (runSpeedX + runSpeedY == 0);
                    var isControllerOriented = !(isStoppedOnGround) && ModSettings.ControllerOrientedMovement;

                    if ((OWInput.GetInputMode() != InputMode.Character))
                    {
                        return;
                    }

                    var rotationSource = isControllerOriented ? LaserPointer.Behaviour.LeftHandLaser : _playerCamera.transform;

                    var fromTo = Quaternion.FromToRotation(_playerBody.transform.forward, Vector3.ProjectOnPlane(rotationSource.transform.forward, _playerBody.transform.up));

                    var magnitude = 0f;
                    if (!isControllerOriented)
                    {
                        var magnitudeUp = 1 - Vector3.ProjectOnPlane(rotationSource.transform.up, _playerBody.transform.up).magnitude;
                        var magnitudeForward = 1 - Vector3.ProjectOnPlane(rotationSource.transform.up, _playerBody.transform.right).magnitude;
                        magnitude = magnitudeUp + magnitudeForward;

                        if (magnitude < 0.3f)
                        {
                            return;
                        }
                    }

                    var targetRotation = fromTo * _playerBody.transform.rotation;
                    var inverseRotation = Quaternion.Inverse(fromTo) * _playArea.rotation;

                    if (isControllerOriented)
                    {
                        _playArea.rotation = inverseRotation;
                        rotationSetter(targetRotation);
                    }
                    else
                    {
                        var maxDegreesDelta = magnitude * 3f;
                        _playArea.rotation = Quaternion.RotateTowards(_playArea.rotation, inverseRotation, maxDegreesDelta);
                        rotationSetter(Quaternion.RotateTowards(_playerBody.transform.rotation, targetRotation, maxDegreesDelta));
                    }
                }
            }

        }
    }
}
