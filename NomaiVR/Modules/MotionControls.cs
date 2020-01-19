using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        Transform _rightHandParent;
        Transform _leftHandParent;
        Transform _debugTransform;
        Transform _wrapper;
        bool _angleMode;

        void Start() {
            NomaiVR.Log("Start MotionControls");

            NomaiVR.Helper.Events.Subscribe<Signalscope>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;

            // For some reason objects are very high up if tracking space is not stationary.
            // Not sure exactly what stationary entails here, since it since tracks position fine.
            //Valve.VR.OpenVR.System.ResetSeatedZeroPose();
            //Valve.VR.OpenVR.Compositor.SetTrackingSpace(
            //Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            //OpenVR.Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseSeated);
            //OpenVR.System.ResetSeatedZeroPose();


            // Set up tracked hand objects
            _wrapper = new GameObject().transform;
            _rightHandParent = CreateHand("PlayerSuit_Glove_Right", SteamVR_Actions.default_RightPose, Quaternion.Euler(45, 180, 0), _wrapper);
            _leftHandParent = CreateHand("PlayerSuit_Glove_Left", SteamVR_Actions.default_LeftPose, Quaternion.Euler(-40, 330, 20), _wrapper);
            _wrapper.parent = Common.MainCamera.transform.parent;
            _wrapper.localRotation = Quaternion.identity;
            _wrapper.localPosition = Common.MainCamera.transform.localPosition;
            //Valve.VR.OpenVR.System.ResetSeatedZeroPose();
            //Valve.VR.OpenVR.Compositor.SetTrackingSpace(
            //Valve.VR.ETrackingUniverseOrigin.TrackingUniverseStanding);

            HoldSignalscope();
            HoldLaunchProbe();
            HoldHUD();
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(Signalscope) && ev == Events.AfterStart) {



            }
        }

        Transform CreateHand(string objectName, SteamVR_Action_Pose pose, Quaternion rotation, Transform wrapper) {
            var hand = Instantiate(GameObject.Find("SpaceSuit").transform.Find("Props_HEA_PlayerSuit_Hanging/" + objectName).gameObject).transform;
            var handParent = new GameObject().transform;
            handParent.parent = wrapper;

            hand.parent = handParent;
            //hand.localPosition = new Vector3(0, -0.03f, -0.08f);
            hand.localPosition = new Vector3(0, -0.03f, -0.08f);
            hand.localRotation = rotation;
            //hand.position = Common.MainCamera.transform.position - hand.position;
            hand.localScale = Vector3.one * 0.5f;

            //handParent.parent = Common.MainCamera.transform.parent;
            //handParent.localPosition = Vector3.zero;
            //handParent.localRotation = Quaternion.identity;

            handParent.gameObject.SetActive(false);
            var poseDriver = handParent.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;
            handParent.gameObject.SetActive(true);



            return hand;
        }

        void HoldHUD() {
            var playerHUD = GameObject.Find("PlayerHUD");
            playerHUD.transform.localScale = Vector3.one * 0.2f;
            playerHUD.transform.localPosition = Vector3.zero;
            playerHUD.transform.localRotation = Quaternion.identity;

            var hudElements = Common.GetObjectsInLayer(playerHUD.gameObject, LayerMask.NameToLayer("HeadsUpDisplay"));

            foreach (var hudElement in hudElements) {
                hudElement.layer = 0;
                hudElement.SetActive(true);
            }

            var uiCanvas = playerHUD.transform.Find("HelmetOnUI/UICanvas").GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.transform.localPosition = Vector3.zero;
            uiCanvas.transform.localRotation = Quaternion.identity;

            HoldObject(playerHUD.transform, _leftHandParent, new Vector3(0.12f, -0.09f, 0.01f), Quaternion.Euler(47f, 220f, 256f));

            //_debugTransform = playerHUD.transform;
        }

        void HoldSignalscope() {
            var signalScope = Common.MainCamera.transform.Find("Signalscope");
            HoldObject(signalScope, _rightHandParent, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));

            var signalScopeModel = signalScope.GetChild(0);
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
            reticule.transform.parent = signalScope;
            reticule.transform.localScale = Vector3.one * 0.0005f;
            reticule.transform.localPosition = Vector3.forward * 0.5f;
            reticule.transform.localRotation = Quaternion.identity;
        }

        void HoldLaunchProbe() {
            var probeLauncher = Common.MainCamera.transform.Find("ProbeLauncher");
            probeLauncher.localScale = Vector3.one * 0.3f;
            HoldObject(probeLauncher, _rightHandParent, new Vector3(-0.05f, 0.16f, 0.05f), Quaternion.Euler(45, 0, 0));

            var probeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
            probeLauncherModel.gameObject.layer = 0;
            probeLauncherModel.localPosition = Vector3.zero;
            probeLauncherModel.localRotation = Quaternion.identity;

            probeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);
            probeLauncherModel.Find("Props_HEA_Probe_Prelaunch/Props_HEA_Probe_Prelaunch_Prepass").gameObject.SetActive(false);

            var renderers = probeLauncher.gameObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var renderer in renderers) {
                if (renderer.name == "RecallEffect") {
                    NomaiVR.Log("found ReacllEffect");
                    continue;
                }
                foreach (var material in renderer.materials) {
                    material.shader = Shader.Find("Standard");
                }
            }

            // This one is used only for rendering the probe launcher to the screen in pancake mode,
            // so we can remove it.
            probeLauncher.Find("Props_HEA_ProbeLauncher_ProbeCamera").gameObject.SetActive(false);

            // This transform defines the origin and direction of the launched probe.
            var launchOrigin = Common.MainCamera.transform.Find("ProbeLauncherTransform").transform;
            launchOrigin.parent = probeLauncherModel;
            launchOrigin.localPosition = Vector3.forward * 0.2f;
            launchOrigin.localRotation = Quaternion.identity;
        }

        void HoldObject(Transform objectTransform, Transform hand, Vector3 position, Quaternion rotation) {
            var objectParent = new GameObject().transform;
            objectParent.parent = hand;
            objectParent.localPosition = position;
            objectParent.localRotation = rotation;
            objectTransform.transform.parent = objectParent;
            objectTransform.transform.localPosition = Vector3.zero;
            objectTransform.transform.localRotation = Quaternion.identity;
        }

        void HoldObject(Transform objectTransform, Transform hand) {
            HoldObject(objectTransform, hand, Vector3.zero, Quaternion.identity);
        }
        void HoldObject(Transform objectTransform, Transform hand, Quaternion rotation) {
            HoldObject(objectTransform, hand, Vector3.zero, rotation);
        }
        void HoldObject(Transform objectTransform, Transform hand, Vector3 position) {
            HoldObject(objectTransform, hand, position, Quaternion.identity);
        }

        void Update() {
            if (_wrapper) {
                _wrapper.localPosition = Common.MainCamera.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
            }
            if (_debugTransform) {
                Vector3 position = _debugTransform.parent.localPosition;
                var posDelta = 0.01f;

                if (!_angleMode) {
                    if (Input.GetKeyDown(KeyCode.Keypad7)) {
                        position.x += posDelta;
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad4)) {
                        position.x -= posDelta;
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad8)) {
                        position.y += posDelta;
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad5)) {
                        position.y -= posDelta;
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad9)) {
                        position.z += posDelta;
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad6)) {
                        position.z -= posDelta;
                    }
                }

                Quaternion rotation = _debugTransform.parent.localRotation;
                float angleDelta = 5;

                if (_angleMode) {
                    if (Input.GetKeyDown(KeyCode.Keypad7)) {
                        rotation = rotation * Quaternion.Euler(angleDelta, 0, 0);
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad4)) {
                        rotation = rotation * Quaternion.Euler(-angleDelta, 0, 0);
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad8)) {
                        rotation = rotation * Quaternion.Euler(0, angleDelta, 0);
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad5)) {
                        rotation = rotation * Quaternion.Euler(0, -angleDelta, 0);
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad9)) {
                        rotation = rotation * Quaternion.Euler(0, 0, angleDelta);
                    }
                    if (Input.GetKeyDown(KeyCode.Keypad6)) {
                        rotation = rotation * Quaternion.Euler(0, 0, -angleDelta);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Keypad0)) {
                    _angleMode = !_angleMode;
                }

                if (Input.anyKeyDown) {
                    _debugTransform.parent.localPosition = position;
                    _debugTransform.parent.localRotation = rotation;
                    var angles = _debugTransform.parent.localEulerAngles;
                    NomaiVR.Log("position: new Vector3(" + position.x + "f, " + position.y + "f, " + position.z + "f)");
                    NomaiVR.Log("Rotation: Quaternion.Euler(" + angles.x + "f, " + angles.y + "f, " + angles.z + "f)");
                }
            }
        }
    }
}
