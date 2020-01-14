using OWML.Common;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        Transform _hand;
        Transform _handParent;
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
            }
        }

        void SetupMotion() {
            var rightHand = Instantiate(GameObject.Find("PlayerSuit_Glove_Right"));
            var handParent = new GameObject();
            _handParent = handParent.transform;
            _hand = rightHand.transform;
            _hand.parent = handParent.transform;
            _hand.localRotation = Quaternion.Euler(45, 180, 0);
            _hand.localPosition = new Vector3(0, -0.03f, -0.08f);
            _hand.localScale = Vector3.one * 0.5f;

            addPoseDriver(handParent);

            var signalScope = GameObject.Find("Signalscope");
            HoldObject(signalScope, new Vector3(-0.035f, 0.017f, 0.106f), Quaternion.Euler(32, 0, 0));
            ////var probeLauncher = GameObject.Find("ProbeLauncher");
            ////setup(signalScope);
            ////setup(probeLauncher);

            var signalScopeModel = signalScope.transform.GetChild(0);
            // Tools have a special shader that draws them on top of everything
            // and screws with perspective. Changing to Standard shader so they look
            // like a normal 3D object.
            signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            signalScopeModel.localPosition = Vector3.zero;
            signalScopeModel.localRotation = Quaternion.identity;

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it doesn't seem necessary.
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
            objectParent.parent = _handParent;
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
