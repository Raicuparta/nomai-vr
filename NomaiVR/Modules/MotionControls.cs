using OWML.Common;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        Transform _rightHandParent;
        Transform _leftHandParent;
        void Start() {
            NomaiVR.Log("Start MotionControls");

            NomaiVR.Helper.Events.Subscribe<Signalscope>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;

            XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
            InputTracking.Recenter();
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(Signalscope) && ev == Events.AfterStart) {
                SetupMotion();
                HoldSignalscope();
            }
        }

        void SetupMotion() {
            //var rightHand = Instantiate(GameObject.Find("PlayerSuit_Glove_Right"));
            //var handParent = new GameObject();
            //_rightHandParent = handParent.transform;
            //rightHand.transform.parent = handParent.transform;
            //rightHand.transform.localRotation = Quaternion.Euler(45, 180, 0);
            //rightHand.transform.localPosition = new Vector3(0, -0.03f, -0.08f);
            //rightHand.transform.localScale = Vector3.one * 0.5f;

            //addPoseDriver(handParent);

            _rightHandParent = CreateHand("PlayerSuit_Glove_Right", TrackedPoseDriver.TrackedPose.RightPose, Quaternion.Euler(45, 180, 0));
            _leftHandParent = CreateHand("PlayerSuit_Glove_Left", TrackedPoseDriver.TrackedPose.LeftPose, Quaternion.Euler(-40, 330, 20));
        }

        Transform CreateHand(string objectName, TrackedPoseDriver.TrackedPose pose, Quaternion rotation) {
            var hand = Instantiate(GameObject.Find("SpaceSuit").transform.Find("Props_HEA_PlayerSuit_Hanging/" + objectName).gameObject).transform;
            var handParent = new GameObject().transform;
            hand.parent = handParent;
            hand.localRotation = rotation;
            hand.localPosition = new Vector3(0, -0.03f, -0.08f);
            hand.localScale = Vector3.one * 0.5f;

            handParent.parent = Common.MainCamera.transform.parent;
            handParent.position = Common.MainCamera.transform.position;
            handParent.localRotation = Quaternion.identity;

            var poseDriver = handParent.gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, pose);

            return handParent;
        }

        void HoldSignalscope() {
            var signalScope = GameObject.Find("Signalscope");
            HoldObject(signalScope, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));

            var signalScopeModel = signalScope.transform.GetChild(0);
            // Tools have a special shader that draws them on top of everything
            // and screws with perspective. Changing to Standard shader so they look
            // like a normal 3D object.
            signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            signalScopeModel.localPosition = Vector3.up * -0.1f;
            signalScopeModel.localRotation = Quaternion.identity;

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it looks glitchy and doesn't seem necessary.
            signalScopeModel.GetChild(0).gameObject.SetActive(false);

            // Attatch Signalscope UI to the Signalscope.
            var reticule = GameObject.Find("SignalscopeReticule").GetComponent<Canvas>();
            reticule.renderMode = RenderMode.WorldSpace;
            reticule.transform.parent = signalScope.transform;
            reticule.transform.localScale = Vector3.one * 0.0005f;
            reticule.transform.localPosition = Vector3.forward * 0.5f;
            reticule.transform.localRotation = Quaternion.identity;
        }

        void HoldObject(GameObject gameObject, Vector3 position, Quaternion rotation) {
            var objectParent = new GameObject().transform;
            objectParent.parent = _rightHandParent;
            objectParent.localPosition = position;
            objectParent.localRotation = rotation;
            gameObject.transform.parent = objectParent;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
        }

        void HoldObject(GameObject gameObject) {
            HoldObject(gameObject, Vector3.zero, Quaternion.identity);
        }
        void HoldObject(GameObject gameObject, Quaternion rotation) {
            HoldObject(gameObject, Vector3.zero, rotation);
        }
        void HoldObject(GameObject gameObject, Vector3 position) {
            HoldObject(gameObject, position, Quaternion.identity);
        }

        void addPoseDriver(GameObject gameObject) {
            gameObject.transform.parent = Common.MainCamera.transform.parent;
            gameObject.transform.position = Common.MainCamera.transform.position;
            gameObject.transform.localRotation = Quaternion.identity;
            var poseDriver = gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
            //poseDriver.UseRelativeTransform = true;
        }
    }
}
