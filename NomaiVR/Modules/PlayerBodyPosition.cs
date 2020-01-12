using OWML.Common;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;

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
        GameObject _signalscope;

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
            NomaiVR.Log("pre_signalscope");


            //XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
            _signalscope = GameObject.Find("Signalscope");
            NomaiVR.Log("_signalscope " + _signalscope.name);
            _signalscope.transform.parent = _cameraParent.transform;
            _signalscope.transform.position = _playerHead.position;
            _signalscope.transform.localRotation = Quaternion.identity;
            //NomaiVR.Log("childCount " + );
            _signalscope.transform.GetChild(0).GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            _signalscope.transform.GetChild(0).localPosition = Vector3.up * -0.1f;
            _signalscope.transform.GetChild(0).localRotation = Quaternion.identity;
            //_signalscope.transform.GetChild(0).GetChild(0).localPosition = Vector3.zero;
            //_signalscope.transform.GetChild(0).GetChild(0).localRotation = Quaternion.identity;
            _signalscope.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            NomaiVR.Log("nullified");
            //_signalscope.GetComponent<Material>().shader = new Shader();
            //NomaiVR.Log("count " + _signalscope.GetComponentsInChildren<Material>().Length);
            //GameObject.Find("Scene").SetActive(false);
            //_signalscope.transform.parent = _mainCamera.transform.parent;
            //_signalscope.transform.DestroyAllChildren();
            //_signalscope.transform.localPosition = Vector3.zero;
            //_signalscope.transform.localRotation = Quaternion.identity;
            //_obj.transform.localScale = Vector3.one * 0.05f;
            var poseDriver = _signalscope.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
            poseDriver.UseRelativeTransform = true;

            var reticule = GameObject.Find("SignalscopeReticule").GetComponent<Canvas>();
            reticule.renderMode = RenderMode.WorldSpace;
            reticule.transform.parent = _signalscope.transform;
            reticule.transform.localScale = Vector3.one * 0.0005f;
            reticule.transform.localPosition = Vector3.forward * 0.5f;
            reticule.transform.localRotation = Quaternion.identity;

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
            _mainCamera.transform.localPosition = Vector3.zero;
            _mainCamera.transform.localRotation = Quaternion.identity;

            // This component is messing with our ability to read the VR camera's rotation.
            // I'm disabling it even though I have no clue what it does ¯\_(ツ)_/¯
            PlayerCameraController playerCameraController = _mainCamera.GetComponent<PlayerCameraController>();
            if (playerCameraController) {
                playerCameraController.enabled = false;
            }
        }

        void MoveCameraToPlayerHead() {
            Vector3 movement = _playerHead.position - _mainCamera.transform.position;
            _cameraParent.transform.position += movement;
        }

        void MovePlayerBodyToCamera() {
            // Move player to camera position.
            Vector3 movement = _prevCameraPosition - (_cameraParent.transform.position - _mainCamera.transform.position);
            _playerBody.transform.position += movement;

            _prevCameraPosition = _cameraParent.transform.position - _mainCamera.transform.position;
        }


        void FixedUpdate() {
            if (_isAwake) {
                MoveCameraToPlayerHead();
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

                playerCam.transform.parent.rotation = Quaternion.Inverse(fromTo) * playerCam.transform.parent.rotation;
                transform.rotation = fromTo * transform.rotation;

                _prevRotation = Quaternion.FromToRotation(transform.forward, Vector3.ProjectOnPlane(playerCam.transform.forward, transform.up));
            }
        }

    }
}
