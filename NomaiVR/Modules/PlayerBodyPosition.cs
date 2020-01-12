using OWML.Common;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;

namespace NomaiVR
{
    public class PlayerBodyPosition : MonoBehaviour
    {
        GameObject _cameraParent;
        Vector3 _prevCameraPosition;
        bool _isAwake;

        void Start() {
            NomaiVR.Log("Start PlayerBodyPosition");

            SceneManager.sceneLoaded += OnSceneLoaded;

            NomaiVR.Helper.Events.Subscribe<Flashlight>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnWakeUp;
        }

        private void OnWakeUp(MonoBehaviour behaviour, Events ev) {
            _isAwake = true;
            NomaiVR.Helper.HarmonyHelper.AddPostfix<PlayerCharacterController>("UpdateTurning", typeof(Patches), "PatchTurning");

            MoveCameraToPlayerHead();

            // Move helmet forward so it is easier to look at the HUD in VR
            FindObjectOfType<HUDHelmetAnimator>().transform.localPosition += Vector3.forward * 0.3f;
            NomaiVR.Log("pre_signalscope");

        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            updateMainCamera();
        }

        private void updateMainCamera() {
            NomaiVR.Helper.Console.WriteLine("Main camera: " + Common.MainCamera.name);

            //Make an empty parent object for moving the camera around.
            _cameraParent = new GameObject();
            _cameraParent.transform.parent = Common.MainCamera.transform.parent;
            _cameraParent.transform.position = Common.MainCamera.transform.position;
            _cameraParent.transform.rotation = Common.MainCamera.transform.rotation;
            Common.MainCamera.transform.parent = _cameraParent.transform;
            Common.MainCamera.transform.localPosition = Vector3.zero;
            Common.MainCamera.transform.localRotation = Quaternion.identity;

            // This component is messing with our ability to read the VR camera's rotation.
            // I'm disabling it even though I have no clue what it does ¯\_(ツ)_/¯
            PlayerCameraController playerCameraController = Common.MainCamera.GetComponent<PlayerCameraController>();
            if (playerCameraController) {
                playerCameraController.enabled = false;
            }
        }

        void MoveCameraToPlayerHead() {
            Vector3 movement = Common.PlayerHead.position - Common.MainCamera.transform.position;
            _cameraParent.transform.position += movement;
        }

        void MovePlayerBodyToCamera() {
            // Move player to camera position.
            Vector3 movement = _prevCameraPosition - (_cameraParent.transform.position - Common.MainCamera.transform.position);
            Common.PlayerBody.transform.position += movement;

            _prevCameraPosition = _cameraParent.transform.position - Common.MainCamera.transform.position;
        }


        void FixedUpdate() {
            if (_isAwake) {
                MoveCameraToPlayerHead();
                MovePlayerBodyToCamera();
            }
        }

        internal static class Patches
        {
            static void PatchTurning(PlayerCharacterController __instance) {
                var playerCam = __instance.GetValue<OWCamera>("_playerCam");
                var transform = __instance.GetValue<Transform>("_transform");

                Quaternion fromTo = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up));

                playerCam.transform.parent.rotation = Quaternion.Inverse(fromTo) * playerCam.transform.parent.rotation;
                transform.rotation = fromTo * transform.rotation;
            }
        }

    }
}
