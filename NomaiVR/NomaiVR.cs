using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;

namespace OWML.NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        Rigidbody _playerBody;
        Camera _mainCamera;
        GameObject _cameraParent;
        Transform _playerHead;
        bool _isAwake;
        Vector3 _prevCameraPosition;

        void Start() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            MoveAllCanvasesToWorldSpace();

            ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            ModHelper.Events.OnEvent += OnWakeUp;
        }

        private void updateMainCamera() {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            ModHelper.Console.WriteLine("Main camera: " + _mainCamera.name);

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

        private void OnWakeUp(MonoBehaviour behaviour, Events ev) {
            _isAwake = true;

            _playerBody = GameObject.Find("Player_Body").GetComponent<Rigidbody>();
            _playerHead = FindObjectOfType<ToolModeUI>().transform;

            // Set initial camera position to player head.
            Vector3 movement = _playerHead.position - _mainCamera.transform.position;
            _cameraParent.transform.position += movement;
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            MovePauseMenuToWorldSpace();
            updateMainCamera();
        }

        void MoveAllCanvasesToWorldSpace() {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

            foreach (Canvas canvas in canvases) {
                canvas.renderMode = RenderMode.WorldSpace;
                Transform originalParent = canvas.transform.parent;
                canvas.transform.parent = Object.FindObjectOfType<Camera>().transform;
                canvas.transform.localPosition = new Vector3(0, 0, 1);
                canvas.transform.localRotation = new Quaternion(0, 0, 0, 1);
                canvas.transform.localScale = Vector3.one * 0.0005f;
            }
        }

        void MovePauseMenuToWorldSpace() {
            GameObject pauseMenu = GameObject.Find("PauseMenu");
            ModHelper.Console.WriteLine("pauseMenu: " + pauseMenu.name);
            Canvas[] canvases = pauseMenu.GetComponentsInChildren<Canvas>();
            Camera playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();

            foreach (Canvas canvas in canvases) {
                canvas.renderMode = RenderMode.WorldSpace;
            }

            pauseMenu.transform.parent = playerCamera.transform;
            pauseMenu.transform.localPosition = new Vector3(-0.6f, -0.5f, 1);
            pauseMenu.transform.localEulerAngles = new Vector3(0, 0, 0);
            pauseMenu.transform.localScale = Vector3.one * 0.001f;
        }

        void MovePlayerBodyToCamera() {
            // Move player to camera position.
            Vector3 movement = _prevCameraPosition - (_playerHead.position - _mainCamera.transform.position);
            _playerBody.transform.localPosition += movement;

            // Since camera is a child of player body, we need to adjust it now.
            Vector3 cameraMovement = _cameraParent.transform.InverseTransformVector(movement);
            cameraMovement.y = 0;
            _cameraParent.transform.localPosition -= cameraMovement;

            _prevCameraPosition = _playerHead.position - _mainCamera.transform.position;
        }

        void Update() {
            if (_isAwake) {
                MovePlayerBodyToCamera();
            }
        }

    }
}