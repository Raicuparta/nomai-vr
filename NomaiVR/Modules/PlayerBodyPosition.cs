using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class PlayerBodyPosition: MonoBehaviour {
        GameObject _cameraParent;
        Vector3 _prevCameraPosition;
        public static bool MovePlayerWithHead = true;

        void Start () {
            NomaiVR.Log("Start PlayerBodyPosition");

            updateMainCamera();
            NomaiVR.Helper.HarmonyHelper.AddPostfix<PlayerCharacterController>("UpdateTurning", typeof(Patches), "PatchTurning");
            MoveCameraToPlayerHead();

            InvokeRepeating("LogMagnitude", 0.5f, 0.5f);
        }

        void LogMagnitude () {
            var magnitude = Vector3.ProjectOnPlane(Camera.main.transform.forward, Common.PlayerBody.transform.up).magnitude;
            NomaiVR.Log("magnitude", magnitude.ToString());
        }

        private void updateMainCamera () {
            NomaiVR.Helper.Console.WriteLine("Main camera: " + Common.MainCamera.name);

            // Make an empty parent object for moving the camera around.
            _cameraParent = new GameObject();
            _cameraParent.transform.parent = Common.MainCamera.transform.parent;
            _cameraParent.transform.localPosition = Vector3.zero;
            _cameraParent.transform.localRotation = Quaternion.identity;
            Common.MainCamera.transform.parent = _cameraParent.transform;
            Common.MainCamera.transform.localPosition = Vector3.zero;
            Common.MainCamera.transform.localRotation = Quaternion.identity;

            // This component is messing with our ability to read the VR camera's rotation.
            // Seems to be responsible for controlling the camera rotation with the mouse / joystick.
            PlayerCameraController playerCameraController = Common.MainCamera.GetComponent<PlayerCameraController>();
            if (playerCameraController) {
                playerCameraController.enabled = false;
            }
        }

        void MoveCameraToPlayerHead () {
            Vector3 movement = Common.PlayerHead.position - Common.MainCamera.transform.position;
            _cameraParent.transform.position += movement;
        }

        void MovePlayerBodyToCamera () {
            Vector3 movement = _prevCameraPosition - (_cameraParent.transform.position - Common.MainCamera.transform.position);
            Common.PlayerBody.transform.position += movement;

            _prevCameraPosition = _cameraParent.transform.position - Common.MainCamera.transform.position;

        }


        void Update () {
            MoveCameraToPlayerHead();
            if (MovePlayerWithHead && OWInput.GetInputMode() == InputMode.Character) {
                MovePlayerBodyToCamera();
            }
            if (NomaiVR.DebugMode) {
                if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                    _cameraParent.transform.localScale *= 0.9f;
                }
                if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                    _cameraParent.transform.localScale /= 0.9f;
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
                var magnitude = Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up).magnitude;

                var targetRotation = fromTo * transform.rotation;
                var inverseRotation = Quaternion.Inverse(fromTo) * playerCam.transform.parent.rotation;

                playerCam.transform.parent.rotation = Quaternion.RotateTowards(playerCam.transform.parent.rotation, inverseRotation, magnitude * magnitude * 2f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, magnitude * magnitude * 2f);
            }
        }

    }
}
