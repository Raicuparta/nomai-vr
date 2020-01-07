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
            ModHelper.Console.WriteLine("Rai Mod Start");

            SceneManager.sceneLoaded += OnSceneLoaded;
            MoveMainMenuToWorldSpace();

            ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            ModHelper.Events.OnEvent += OnWakeUp;

            updateMainCamera();
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

            _prevCameraPosition = _playerHead.position - _mainCamera.transform.position;

            //_initialAngles = _mainCamera.transform.eulerAngles;
        }

        private void OnWakeUp(MonoBehaviour behaviour, Events ev) {
            ModHelper.Console.WriteLine("Wake up");

            _isAwake = true;

            _playerBody = GameObject.Find("Player_Body").GetComponent<Rigidbody>();
            //_playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
            _playerHead = GameObject.FindObjectOfType<ToolModeUI>().transform;
            Vector3 movement = _playerHead.position - _mainCamera.transform.position;
            _cameraParent.transform.position += movement;
            ModHelper.Console.WriteLine("movement " + movement);
        }

        void debug () {
            //ModHelper.Console.WriteLine(_playerCamera.transform.eulerAngles.ToString());
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ModHelper.Console.WriteLine("Scene Loaded: " + scene.name);
            updateMainCamera();
            MovePauseMenuToWorldSpace();
        }

        void MoveMainMenuToWorldSpace() {
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

        void Update() {
            if (_isAwake) {
                Vector3 playerAngles = _playerBody.transform.eulerAngles;
                Vector3 mainAngles = _mainCamera.transform.eulerAngles;
                //Vector3 parentAngles = _cameraParent.transform.eulerAngles;

                //float yAngleOffset = mainAngles.y - _initialAngles.y;

                //Vector3 newAngles = new Vector3(playerAngles.x, playerAngles.x + yAngleOffset, playerAngles.z);


                //_playerBody.MoveRotation(Quaternion.Euler(newAngles));
                //_playerBody.transform.eulerAngles = newAngles;
                //_playerBody.transform.RotateAround(_playerBody.position, _playerBody.transform.up, 0.1f);
                //_playerBody.transform.Rotate(_playerBody.transform.up, 0.1f);
                //_playerBody.MoveRotation(Quaternion.LookRotation(_mainCamera.transform.forward, _playerBody.transform.up));
                //_cameraParent.transform.eulerAngles -= Vector3.up * yAngleOffset;
                //_playerCamera.transform.localEulerAngles = _playerBody.transform.localEulerAngles;

                Vector3 movement = _prevCameraPosition - (_playerHead.position - _mainCamera.transform.position);
                _playerBody.transform.localPosition += movement;
                Vector3 cameraMovement = _cameraParent.transform.InverseTransformVector(movement);
                cameraMovement.y = 0;
                _cameraParent.transform.localPosition -= cameraMovement;



                //_playerBody.MovePosition(_playerBody.transform.localPosition + movement);

                _prevCameraPosition = _playerHead.position - _mainCamera.transform.position;
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                _isAwake = !_isAwake;
                ModHelper.Console.WriteLine("_prevCameraPosition " + _prevCameraPosition);
                ModHelper.Console.WriteLine("_mainCamera.transform.localPosition " + _mainCamera.transform.localPosition);
            }
        }

    }
}