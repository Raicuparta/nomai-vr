using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class PlayerBodyPosition: MonoBehaviour {
        Transform _cameraParent;
        static Transform _playArea;
        Transform _camera;
        public static bool MovePlayerWithHead = true;

        void Start () {
            NomaiVR.Log("Start PlayerBodyPosition");

            NomaiVR.Helper.HarmonyHelper.AddPostfix<PlayerCharacterController>("UpdateTurning", typeof(Patches), "PatchTurning");

            // This component is messing with our ability to read the VR camera's rotation.
            // Seems to be responsible for controlling the camera rotation with the mouse / joystick.
            PlayerCameraController playerCameraController = Camera.main.GetComponent<PlayerCameraController>();
            if (playerCameraController) {
                playerCameraController.enabled = false;
            }

            SetupCamera();
        }

        private void SetupCamera () {
            // Make an empty parent object for moving the camera around.
            _camera = Camera.main.transform;
            _cameraParent = new GameObject().transform;
            _playArea = new GameObject().transform;
            _playArea.parent = Common.PlayerBody.transform;
            _playArea.position = Common.PlayerHead.position;
            _playArea.rotation = Common.PlayerHead.rotation;
            _cameraParent.parent = _playArea;
            _cameraParent.localRotation = Quaternion.identity;
            _camera.parent = _cameraParent;

            Vector3 movement = Common.PlayerHead.position - _camera.position;
            _cameraParent.position += movement;
        }

        void MoveCameraToPlayerHead () {
            Vector3 movement = Common.PlayerHead.position - _camera.position;
            _cameraParent.position += movement;

        }

        void Update () {
            if (Vector3.Distance(Common.PlayerHead.position, _camera.position) > 0.5f) {
                MoveCameraToPlayerHead();
            }
            if (NomaiVR.DebugMode) {
                if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                    _cameraParent.localScale *= 0.9f;
                }
                if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                    _cameraParent.localScale /= 0.9f;
                }
            }
        }

        internal static class Patches {
            static void PatchTurning (PlayerCharacterController __instance) {
                if (OWInput.GetInputMode() != InputMode.Character) {
                    return;
                }

                var playerCam = __instance.GetValue<OWCamera>("_playerCam");
                var transform = __instance.GetValue<Transform>("_transform");

                Quaternion fromTo = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up));

                var magnitudeUp = 1 - Vector3.ProjectOnPlane(playerCam.transform.up, transform.up).magnitude;
                var magnitudeForward = 1 - Vector3.ProjectOnPlane(playerCam.transform.up, transform.right).magnitude;
                var magnitude = magnitudeUp + magnitudeForward;

                if (magnitude < 0.35) {
                    return;
                }

                var targetRotation = fromTo * transform.rotation;
                var inverseRotation = Quaternion.Inverse(fromTo) * _playArea.rotation;

                var maxDegreesDelta = magnitude * 5f;
                _playArea.rotation = Quaternion.RotateTowards(_playArea.rotation, inverseRotation, maxDegreesDelta);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta);
            }
        }

    }
}
