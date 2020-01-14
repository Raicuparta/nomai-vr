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

            // For some reason objects are very high up if tracking space is not stationary.
            // Not sure exactly what stationary entails here, since it since tracks position fine.
            XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
            InputTracking.Recenter();
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(Signalscope) && ev == Events.AfterStart) {
                // Set up tracked hand objects
                _rightHandParent = CreateHand("PlayerSuit_Glove_Right", TrackedPoseDriver.TrackedPose.RightPose, Quaternion.Euler(45, 180, 0));
                _leftHandParent = CreateHand("PlayerSuit_Glove_Left", TrackedPoseDriver.TrackedPose.LeftPose, Quaternion.Euler(-40, 330, 20));

                HoldSignalscope();
                HoldLaunchProbe();
                //var probeLauncherModel = Common.MainCamera.transform.Find("ProbeLauncher/Props_HEA_ProbeLauncher_ProbeCamera");
                //probeLauncherModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            }
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
            HoldObject(signalScope.transform, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));

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

        void HoldLaunchProbe() {
            var probeLauncher = Common.MainCamera.transform.Find("ProbeLauncher");
            probeLauncher.localScale = Vector3.one * 0.4f;
            HoldObject(probeLauncher);

            var probeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
            probeLauncherModel.gameObject.layer = 0;
            probeLauncherModel.localPosition = Vector3.zero;
            probeLauncherModel.localRotation = Quaternion.identity;

            probeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);

            var renderers = probeLauncherModel.GetComponentsInChildren<MeshRenderer>();

            foreach (var renderer in renderers) {
                if (renderer.name == "RecallEffect") {
                    continue;
                }
                renderer.material.shader = Shader.Find("Standard");
            }

            //NomaiVR.Log("shader0  " + probeLauncherModel.GetChild(0).GetComponent<MeshRenderer>().material.shader.name);
            //NomaiVR.Log("shader0  " + probeLauncherModel.GetChild(1).GetComponent<MeshRenderer>().material.shader.name);

            // This one is used only for rendering the probe launcher to the screen in pancake mode,
            // so we can remove it.
            probeLauncher.Find("Props_HEA_ProbeLauncher_ProbeCamera").gameObject.SetActive(false);
        }

        void HoldObject(Transform objectTransform, Vector3 position, Quaternion rotation) {
            var objectParent = new GameObject().transform;
            objectParent.parent = _rightHandParent;
            objectParent.localPosition = position;
            objectParent.localRotation = rotation;
            objectTransform.transform.parent = objectParent;
            objectTransform.transform.localPosition = Vector3.zero;
            objectTransform.transform.localRotation = Quaternion.identity;
        }

        void HoldObject(Transform objectTransform) {
            HoldObject(objectTransform, Vector3.zero, Quaternion.identity);
        }
        void HoldObject(Transform objectTransform, Quaternion rotation) {
            HoldObject(objectTransform, Vector3.zero, rotation);
        }
        void HoldObject(Transform objectTransform, Vector3 position) {
            HoldObject(objectTransform, position, Quaternion.identity);
        }

        void Update() {
            if (_rightHandParent) {
                //var probeLauncher = Common.MainCamera.transform.Find("ProbeLauncher");
                //probeLauncher.position = _rightHandParent.position;
                //probeLauncher.rotation = _rightHandParent.rotation;

                //var probeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher_ProbeCamera");
                //probeLauncherModel.gameObject.layer = 0;
                //probeLauncherModel.localPosition = Vector3.zero;
                //probeLauncherModel.localRotation = Quaternion.identity;
            }
        }
    }
}
