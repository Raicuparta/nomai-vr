using OWML.Common;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR
{
    public class PlayerBodyPosition : MonoBehaviour
    {
        PlayerCharacterController _playerBody;
        Camera _mainCamera;
        GameObject _cameraParent;
        Transform _playerHead;
        bool _isAwake;
        Vector3 _prevCameraPosition;

        void Start() {
            NomaiVR.Log("Start PlayerBodyPosition");

            SceneManager.sceneLoaded += OnSceneLoaded;

            NomaiVR.Helper.Events.Subscribe<Flashlight>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnWakeUp;
        }

        private void OnWakeUp(MonoBehaviour behaviour, Events ev) {
            _isAwake = true;

            _playerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            NomaiVR.Helper.HarmonyHelper.AddPostfix<PlayerCharacterController>("UpdateTurning", typeof(Patches), "PatchTurning");

            _playerHead = FindObjectOfType<ToolModeUI>().transform;

            MoveCameraToPlayerHead();

            // Move helmet forward so it is easier to look at the HUD in VR
            FindObjectOfType<HUDHelmetAnimator>().transform.localPosition += Vector3.forward * 0.3f;
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            updateMainCamera();
        }

        private void updateMainCamera() {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            NomaiVR.Helper.Console.WriteLine("Main camera: " + _mainCamera.name);

            //Make an empty parent object for moving the camera around.
            _cameraParent = new GameObject();
            _cameraParent.transform.parent = _mainCamera.transform.parent;
            _cameraParent.transform.position = _mainCamera.transform.position;
            _cameraParent.transform.rotation = _mainCamera.transform.rotation;
            _mainCamera.transform.parent = _cameraParent.transform;

            // This component is messing with our ability to read the VR camera's rotation.
            // I'm disabling it even though I have no clue what it does ¯\_(ツ)_/¯
            PlayerCameraController playerCameraController = _mainCamera.GetComponent<PlayerCameraController>();
            if (playerCameraController) {
                playerCameraController.enabled = false;
            }
        }

        void MoveCameraToPlayerHead(bool ignoreVerticalAxis = false) {
            Vector3 movement = _playerHead.position - _mainCamera.transform.position;
            float localY = _cameraParent.transform.localPosition.y;
            Vector3 cameraMovement = _cameraParent.transform.InverseTransformVector(movement);

            if (ignoreVerticalAxis) {
                cameraMovement.y = 0;
            }

            _cameraParent.transform.position += movement;
        }

        void MovePlayerBodyToCamera() {
            MoveCameraToPlayerHead(true);

            // Move player to camera position.
            Vector3 movement = _prevCameraPosition - (_cameraParent.transform.position - _mainCamera.transform.position);
            _playerBody.transform.position += movement;

            _prevCameraPosition = _cameraParent.transform.position - _mainCamera.transform.position;
        }


        void FixedUpdate() {
            if (_isAwake) {
                MovePlayerBodyToCamera();
            }
        }

        internal static class Patches
        {
            static Quaternion _prevRotation;

            static void PatchTurning(PlayerCharacterController __instance) {
                var playerCam = __instance.GetValue<OWCamera>("_playerCam");
                var transform = __instance.GetValue<Transform>("_transform");

                Quaternion fromTo = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up));
                
                Quaternion extraTurning = Quaternion.Inverse(_prevRotation) * fromTo;

                playerCam.transform.parent.rotation = Quaternion.Inverse(fromTo) * playerCam.transform.parent.rotation;
                transform.rotation = fromTo * transform.rotation;

                _prevRotation = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up));
            }
        }

    }
}