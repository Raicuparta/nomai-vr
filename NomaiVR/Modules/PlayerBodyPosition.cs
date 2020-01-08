using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OWML.NomaiVR
{
    public class PlayerBodyPosition : MonoBehaviour
    {
        Rigidbody _playerBody;
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

            _playerBody = GameObject.Find("Player_Body").GetComponent<Rigidbody>();
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
            _cameraParent.transform.localPosition = Vector3.zero;
            _cameraParent.transform.localRotation = Quaternion.identity;
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

            _cameraParent.transform.localPosition += cameraMovement;
        }

        void MovePlayerBodyToCamera() {
            // Move player to camera position.
            Vector3 movement = _prevCameraPosition - (_playerHead.position - _mainCamera.transform.position);
            _playerBody.transform.localPosition += movement;

            // Since camera is a child of player body, it also moves when we move the camera.
            // So we need to move the camera to the player's head again.
            MoveCameraToPlayerHead(true);
            
            _prevCameraPosition = _playerHead.position - _mainCamera.transform.position;
        }

        void Update() {
            if (_isAwake) {
                MovePlayerBodyToCamera();
            }
        }

    }
}