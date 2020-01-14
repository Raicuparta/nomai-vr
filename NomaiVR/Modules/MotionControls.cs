using OWML.Common;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        Transform _hand;
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
            _hand = rightHand.transform;
            _hand.parent = handParent.transform;
            _hand.localPosition = Vector3.zero;
            _hand.localRotation = Quaternion.Euler(45, 180, 0);
            _hand.localScale = Vector3.one * 0.5f;

            //var hand = new GameObject();
            //_hand = hand.transform;
            //var filter = hand.AddComponent<MeshFilter>();
            //filter.mesh = (Resources.Load("Mesh/PlayerSuit_Glove_Right.asset", typeof(GameObject)) as GameObject).GetComponent<MeshFilter>().mesh;
            //NomaiVR.Log("mesh: " + filter.mesh.name);
            //var renderer = hand.AddComponent<MeshRenderer>();
            //renderer.material = Resources.Load("Material/Traveller_HEA_PlayerSuit_mat.mat", typeof(Material)) as Material;
            //NomaiVR.Log("material: " + renderer.material.name);
            setup(handParent);


            //var signalScope = GameObject.Find("Signalscope");
            ////var probeLauncher = GameObject.Find("ProbeLauncher");
            ////setup(signalScope);
            ////setup(probeLauncher);

            //var signalScopeModel = signalScope.transform.GetChild(0);
            //// Tools have a special shader that draws them on top of everything
            //// and screws with perspective. Changing to Standard shader so they look
            //// like a normal 3D object.
            //signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            //signalScopeModel.localPosition = Vector3.up * -0.1f;
            //signalScopeModel.localRotation = Quaternion.identity;

            //// This child seems to be only for some kind of shader effect.
            //// Disabling it since it doesn't seem necessary.
            //signalScopeModel.GetChild(0).gameObject.SetActive(false);

            ////addPoseDriver(probeLauncher);
            ////addPoseDriver(signalScope);
            addPoseDriver(handParent);

            //// Attatch Signalscope UI to the Signalscope.
            //var reticule = GameObject.Find("SignalscopeReticule").GetComponent<Canvas>();
            //reticule.renderMode = RenderMode.WorldSpace;
            //reticule.transform.parent = signalScope.transform;
            //reticule.transform.localScale = Vector3.one * 0.0005f;
            //reticule.transform.localPosition = Vector3.forward * 0.5f;
            //reticule.transform.localRotation = Quaternion.identity;
        }

        void setup(GameObject gameObject) {
            gameObject.transform.parent = Common.MainCamera.transform.parent;
            gameObject.transform.position = Common.MainCamera.transform.position;
            gameObject.transform.localRotation = Quaternion.identity;
        }

        void addPoseDriver(GameObject gameObject) {
            var poseDriver = gameObject.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
            //poseDriver.UseRelativeTransform = true;
        }

    }
}
