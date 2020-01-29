using OWML.Common;
using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR {
    public class MotionControls: MonoBehaviour {
        public static Transform RightHand;
        public static Transform LeftHand;
        Transform _debugTransform;
        Transform _handsWrapper;
        bool _angleMode;
        bool _enableLaser = false;
        bool _handNearHead = false;

        void Start () {
            NomaiVR.Log("Start MotionControls");

            NomaiVR.Helper.Events.Subscribe<Signalscope>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;

            // For aiming at interactibles with hand:
            //NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(Patches), "PatchUpdateInteractVolume
        }

        private void OnEvent (MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(Signalscope) && ev == Events.AfterStart) {
                _handsWrapper = new GameObject().transform;
                RightHand = CreateHand("PlayerSuit_Glove_Right", SteamVR_Actions.default_RightPose, Quaternion.Euler(45, 180, 0), _handsWrapper);
                LeftHand = CreateHand("PlayerSuit_Glove_Left", SteamVR_Actions.default_LeftPose, Quaternion.Euler(-40, 330, 20), _handsWrapper);
                _handsWrapper.parent = Common.MainCamera.transform.parent;
                _handsWrapper.localRotation = Quaternion.identity;
                _handsWrapper.localPosition = Common.MainCamera.transform.localPosition;

                if (_enableLaser) {
                    var laser = new GameObject("Laser");
                    laser.transform.parent = RightHand;
                    laser.transform.position = Vector3.zero;
                    laser.transform.rotation = Quaternion.identity;
                    var lineRenderer = RightHand.gameObject.AddComponent<LineRenderer>();
                    lineRenderer.useWorldSpace = false;
                    lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.forward * 3 });
                    lineRenderer.endColor = Color.clear;
                    lineRenderer.startColor = Color.cyan;
                    lineRenderer.material.shader = Shader.Find("Particles/Alpha Blended Premultiply");
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                }

                HideArms();
                gameObject.AddComponent<HoldHUD>();
                gameObject.AddComponent<HoldMallowStick>();
                gameObject.AddComponent<HoldProbeLauncher>();
                gameObject.AddComponent<HoldTranslator>();
                gameObject.AddComponent<HoldSignalscope>();
            }
        }

        Transform CreateHand (string objectName, SteamVR_Action_Pose pose, Quaternion rotation, Transform wrapper) {
            var hand = Instantiate(GameObject.Find("SpaceSuit").transform.Find("Props_HEA_PlayerSuit_Hanging/" + objectName).gameObject).transform;
            var handParent = new GameObject().transform;
            handParent.parent = wrapper;
            hand.parent = handParent;
            hand.localPosition = new Vector3(0, -0.03f, -0.08f);
            hand.localRotation = rotation;
            hand.localScale = Vector3.one * 0.5f;

            handParent.gameObject.SetActive(false);
            var poseDriver = handParent.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;
            handParent.gameObject.SetActive(true);

            return handParent;
        }

        void HideArms () {
            var palyerMeshes = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");

            var suitMesh = palyerMeshes.Find("Traveller_Mesh_v01:Traveller_Geo");
            suitMesh.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject.SetActive(false);
            suitMesh.Find("Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject.SetActive(false);
            suitMesh.Find("Traveller_Mesh_v01:Props_HEA_Jetpack").gameObject.SetActive(false);

            var bodyMesh = palyerMeshes.Find("player_mesh_noSuit:Traveller_HEA_Player");
            bodyMesh.Find("player_mesh_noSuit:Player_RightArm").gameObject.SetActive(false);
            bodyMesh.Find("player_mesh_noSuit:Player_LeftArm").gameObject.SetActive(false);
        }

        public static void HoldObject (Transform objectTransform, Transform hand, Vector3 position, Quaternion rotation) {
            var objectParent = new GameObject().transform;
            objectParent.parent = hand;
            objectParent.localPosition = position;
            objectParent.localRotation = rotation;
            objectTransform.transform.parent = objectParent;
            objectTransform.transform.localPosition = Vector3.zero;
            objectTransform.transform.localRotation = Quaternion.identity;

            var tool = objectTransform.gameObject.GetComponent<PlayerTool>();
            if (tool) {
                tool.SetValue("_stowTransform", null);
                tool.SetValue("_holdTransform", null);
            }
        }

        public static void HoldObject (Transform objectTransform, Transform hand) {
            HoldObject(objectTransform, hand, Vector3.zero, Quaternion.identity);
        }
        public static void HoldObject (Transform objectTransform, Transform hand, Quaternion rotation) {
            HoldObject(objectTransform, hand, Vector3.zero, rotation);
        }
        public static void HoldObject (Transform objectTransform, Transform hand, Vector3 position) {
            HoldObject(objectTransform, hand, position, Quaternion.identity);
        }

        void UpdateFlashlightGesture () {
            if (RightHand) {
                var templePosition = Common.PlayerHead.position + Common.PlayerHead.right * 0.15f;
                if (!_handNearHead && Vector3.Distance(RightHand.position, templePosition) < 0.15f) {
                    _handNearHead = true;
                    ControllerInput.SimulateButton(XboxButton.RightStickClick, 1);
                }
                if (_handNearHead && Vector3.Distance(RightHand.position, templePosition) > 0.16f) {
                    _handNearHead = false;
                    ControllerInput.SimulateButton(XboxButton.RightStickClick, 0);
                }
            }
        }

        void UpdateHandPosition () {
            if (_handsWrapper) {
                _handsWrapper.localPosition = Common.MainCamera.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
            }
        }

        void UpdateDebugTransform () {
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

        void Update () {
            UpdateHandPosition();
            UpdateFlashlightGesture();
            UpdateDebugTransform();
        }

        internal static class Patches {
            static bool PatchUpdateInteractVolume (
                InteractZone __instance,
                OWCamera ____playerCam,
                float ____viewingWindow,
                ref bool ____focused
            ) {
                float num = 2f * Vector3.Angle(MotionControls.RightHand.forward, __instance.transform.forward);
                ____focused = (num <= ____viewingWindow);
                var Base = __instance as SingleInteractionVolume;

                var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action) Activator.CreateInstance(typeof(Action), __instance, ftn);

                func();

                return false;
            }
        }
    }
}
