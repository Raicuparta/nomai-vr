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
        Vector3 _prevCameraForward;

        void Start() {
            NomaiVR.Log("Start PlayerBodyPosition");

            SceneManager.sceneLoaded += OnSceneLoaded;

            NomaiVR.Helper.Events.Subscribe<Flashlight>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnWakeUp;
        }

        private void OnWakeUp(MonoBehaviour behaviour, Events ev) {
            _isAwake = true;

            _playerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerCharacterController>("UpdateTurning", typeof(Patches), "PatchTurning");

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
            //_cameraParent.transform.parent = _mainCamera.transform.parent;
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

            // Since camera is a child of player body, it also moves when we move the camera.
            // So we need to move the camera to the player's head again.
            
            _prevCameraPosition = _cameraParent.transform.position - _mainCamera.transform.position;
        }

        float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);

            return dir;
        }

        void FixedUpdate() {
            if (_isAwake) {
                MovePlayerBodyToCamera();
                //_cameraParent.transform.rotation = _playerHead.rotation;

                //float deltaAngle = Vector3.Angle(Vector3.ProjectOnPlane(_mainCamera.transform.right, _playerBody.transform.up), Vector3.ProjectOnPlane(_playerHead.transform.right, _playerBody.transform.up));

                //if (deltaAngle > 0) {
                //    _playerBody.SetValue("_currentAngularVelocity", 10f);
                //} else if (deltaAngle < 0) {
                //    _playerBody.SetValue("_currentAngularVelocity", -10f);
                //} else {
                //    _playerBody.SetValue("_currentAngularVelocity", 0f);
                //}

                //_prevCameraForward = _cameraParent.transform.right - 


                //Vector3 forward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, _playerBody.transform.up);
                //_playerBody.transform.forward = forward;

                //_cameraParent.transform.up = _playerBody.transform.up;

                //_playerBody.transform.rotation = _playerBody.rotation * Quaternion.AngleAxis(deltaAngle, _playerBody.transform.up);
                //_playerBody.MoveRotation(_playerBody.rotation * Quaternion.AngleAxis(deltaAngle, _playerBody.transform.up));
                //_playerBody.MoveRotation(Quaternion.LookRotation(forward, _playerBody.transform.up));
                //_playerBody.SetAngularVelocity(Vector3.one * 100f);

                //Vector3 difference = _playerBody.transform.forward - _mainCamera.transform.forward;


                //if (deltaAngle > 0) {
                //    Patches.TurnDirection = 1;
                //} else if (deltaAngle < 0) {
                //    Patches.TurnDirection = -1;
                //} else {
                //    Patches.TurnDirection = 0;
                //}
                float dot = Vector3.Dot(
                    Vector3.ProjectOnPlane(_mainCamera.transform.forward, _playerBody.transform.up),
                    _playerBody.transform.forward
                );

                Patches.TurnDirection = AngleDir(_playerBody.transform.forward, _mainCamera.transform.forward, _playerBody.transform.up);

            }
        }

        internal static class Patches
        {
            public static float TurnDirection = 0;
            static bool PatchTurning(PlayerCharacterController __instance) {
                var playerCam = __instance.GetValue<OWCamera>("_playerCam");
                var transform = __instance.GetValue<Transform>("_transform");

                Quaternion extraTurning = Quaternion.AngleAxis(TurnDirection * 10, transform.up);

                float num = 1f;
                num *= playerCam.fieldOfView / __instance.GetValue<float>("_initFOV");
                float num2 = OWInput.GetValue(InputLibrary.look, InputMode.Character | InputMode.ScopeZoom | InputMode.NomaiRemoteCam).x * num;
                __instance.SetValue("_lastTurnInput", num2);
                float num3 = (!__instance.GetValue<bool>("_signalscopeZoom")) ? PlayerCameraController.LOOK_RATE : (PlayerCameraController.LOOK_RATE * PlayerCameraController.ZOOM_SCALAR);
                float angle = num2 * num3 * Time.fixedDeltaTime / Time.timeScale;
                Quaternion lhs = Quaternion.AngleAxis(angle, transform.up);

                var movingPlatform = __instance.GetValue<MovingPlatform>("_movingPlatform");
                var groudBody = __instance.GetValue<OWRigidbody>("_groundBody");
                var baseAngularVelocity = __instance.GetValue<float>("_baseAngularVelocity");

                if (__instance.GetValue<bool>("_isGrounded") && groudBody != null) {
                    Vector3 vector = (!(movingPlatform != null)) ? groudBody.GetAngularVelocity() : movingPlatform.GetAngularVelocity();
                    int num4 = (int)Mathf.Sign(Vector3.Dot(vector, transform.up));
                    __instance.SetValue("_baseAngularVelocity", Vector3.Project(vector, transform.up).magnitude * (float)num4);
                } else {
                    __instance.SetValue("_baseAngularVelocity", baseAngularVelocity * 0.995f);
                }
                Quaternion rhs = Quaternion.AngleAxis(baseAngularVelocity * 180f / 3.14159274f * Time.fixedDeltaTime, transform.up);

                transform.rotation = extraTurning * lhs * rhs * transform.rotation;
                playerCam.transform.parent.rotation = lhs * rhs * playerCam.transform.parent.rotation;

                return false;
            }
        }

    }
}