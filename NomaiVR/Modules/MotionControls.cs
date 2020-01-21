using OWML.Common;
using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        public static Transform _rightHandParent;
        protected static ProbeLauncherUI ProbeUI;
        Transform _leftHandParent;
        Transform _debugTransform;
        Transform _wrapper;
        bool _angleMode;

        void Start() {
            NomaiVR.Log("Start MotionControls");

            NomaiVR.Helper.Events.Subscribe<Signalscope>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(Signalscope) && ev == Events.AfterStart) {
                _wrapper = new GameObject().transform;
                _rightHandParent = CreateHand("PlayerSuit_Glove_Right", SteamVR_Actions.default_RightPose, Quaternion.Euler(45, 180, 0), _wrapper);
                _leftHandParent = CreateHand("PlayerSuit_Glove_Left", SteamVR_Actions.default_LeftPose, Quaternion.Euler(-40, 330, 20), _wrapper);
                _wrapper.parent = Common.MainCamera.transform.parent;
                _wrapper.localRotation = Quaternion.identity;
                _wrapper.localPosition = Common.MainCamera.transform.localPosition;

                HoldSignalscope();
                HoldLaunchProbe();
                HoldTranslator();
                HoldHUD();

                // For aiming at interactibles with hand:
                //NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(Patches), "PatchUpdateInteractVolume");

                // For fixing signalscope zoom
                //NomaiVR.Helper.HarmonyHelper.AddPostfix<Signalscope>("EnterSignalscopeZoom", typeof(Patches), "ZoomIn");
                //NomaiVR.Helper.HarmonyHelper.AddPostfix<Signalscope>("ExitSignalscopeZoom", typeof(Patches), "ZoomOut");
                //behaviour.SetValue("_targetFOV", Common.MainCamera.fieldOfView);

                NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerSpacesuit>("SuitUp", typeof(Patches), "SuitUp");
                NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerSpacesuit>("RemoveSuit", typeof(Patches), "RemoveSuit");
            }
        }

        Transform CreateHand(string objectName, SteamVR_Action_Pose pose, Quaternion rotation, Transform wrapper) {
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

            var signalScopeHolster = Instantiate(signalScopeModel).gameObject;
            signalScopeHolster.SetActive(true);
            var holster = signalScopeHolster.AddComponent<ToolHolster>();
            holster.hand = _rightHandParent;
            holster.offset = 0.35f;
            holster.mode = ToolMode.SignalScope;
            holster.scale = 0.8f;


            var playerHUD = GameObject.Find("PlayerHUD").transform;
            var reticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");
            var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
            var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");


            // Attatch Signalscope UI to the Signalscope.
            reticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            reticule.parent = signalScope;
            reticule.localScale = Vector3.one * 0.0005f;
            reticule.localPosition = Vector3.forward * 0.5f;
            reticule.localRotation = Quaternion.identity;

            helmetOff.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOff.parent = signalScope;
            helmetOff.localScale = Vector3.one * 0.0005f;
            helmetOff.localPosition = Vector3.zero;
            helmetOff.localRotation = Quaternion.identity;

            helmetOn.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOn.parent = signalScope;
            helmetOn.localScale = Vector3.one * 0.0005f;
            helmetOn.localPosition = Vector3.down * 0.5f;
            helmetOn.localRotation = Quaternion.identity;
        }

        void HoldLaunchProbe() {
            var probeLauncher = Common.MainCamera.transform.Find("ProbeLauncher");
            probeLauncher.localScale = Vector3.one * 0.2f;
            HoldObject(probeLauncher, _rightHandParent, new Vector3(-0.04f, 0.09f, 0.03f), Quaternion.Euler(45, 0, 0));

            var probeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
            probeLauncherModel.gameObject.layer = 0;
            probeLauncherModel.localPosition = Vector3.zero;
            probeLauncherModel.localRotation = Quaternion.identity;

            probeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);
            probeLauncherModel.Find("Props_HEA_Probe_Prelaunch/Props_HEA_Probe_Prelaunch_Prepass").gameObject.SetActive(false);

            var renderers = probeLauncher.gameObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var renderer in renderers) {
                if (renderer.name == "RecallEffect") {
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

            var probeLauncherHolster = Instantiate(probeLauncherModel).gameObject;
            probeLauncherHolster.SetActive(true);
            var holster = probeLauncherHolster.AddComponent<ToolHolster>();
            holster.hand = _rightHandParent;
            holster.offset = 0.1f;
            holster.mode = ToolMode.Probe;
            holster.scale = 0.15f;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            var display = playerHUD.Find("HelmetOffUI/ProbeDisplay");
            display.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            display.parent = probeLauncherModel;
            display.localScale = Vector3.one * 0.0012f;
            display.localRotation = Quaternion.identity;
            display.localPosition = Vector3.forward * -0.67f;
            ProbeUI = display.GetComponent<ProbeLauncherUI>();
            //var clone = Instantiate(display);
            //NomaiVR.Log("cloned " + clone.name);
            //var cloneUI = clone.GetComponent<ProbeLauncherUI>();
            //NomaiVR.Log("cloned UI " + cloneUI.name);
            //cloneUI.SetValue("_nonSuitUI", false);

            var displayImage = display.GetChild(0).GetComponent<RectTransform>();
            displayImage.anchorMin = Vector2.one * 0.5f;
            displayImage.anchorMax = Vector2.one * 0.5f;
            displayImage.pivot = Vector2.one * 0.5f;
            displayImage.localPosition = Vector3.zero;
            displayImage.localRotation = Quaternion.identity;
        }

        void HoldTranslator() {
            var translator = Common.MainCamera.transform.Find("NomaiTranslatorProp");
            HoldObject(translator, _rightHandParent, new Vector3(-0.24f, 0.08f, 0.06f), Quaternion.Euler(32.8f, 0f, 0f));

            _debugTransform = translator;

            var translatorGroup = translator.Find("TranslatorGroup");
            translatorGroup.localPosition = Vector3.zero;
            translatorGroup.localRotation = Quaternion.identity;

            translator.localScale = Vector3.one * 0.3f;
            var translatorModel = translatorGroup.Find("Props_HEA_Translator");
            translatorModel.localPosition = Vector3.zero;
            translatorModel.localRotation = Quaternion.identity;

            translator.GetComponent<NomaiTranslator>().SetValue("_raycastTransform", translatorModel);

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it looks glitchy and doesn't seem necessary.
            translatorModel.Find("Props_HEA_Translator_Prepass").gameObject.SetActive(false);

            var renderers = translatorModel.gameObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var renderer in renderers) {
                foreach (var material in renderer.materials) {
                    material.shader = Shader.Find("Standard");
                }
            }

            var texts = translator.gameObject.GetComponentsInChildren<Graphic>(true);

            foreach (var text in texts) {
                text.material = null;
            }

            var signalScopeHolster = Instantiate(translatorModel).gameObject;
            signalScopeHolster.SetActive(true);
            var holster = signalScopeHolster.AddComponent<ToolHolster>();
            holster.hand = _rightHandParent;
            holster.offset = -0.3f;
            holster.mode = ToolMode.Translator;
            holster.scale = 0.15f;
        }

        void HoldObject(Transform objectTransform, Transform hand, Vector3 position, Quaternion rotation) {
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

        internal static class Patches
        {
            static bool PatchUpdateInteractVolume(
                InteractZone __instance,
                OWCamera ____playerCam,
                float ____viewingWindow,
                ref bool ____focused
            ) {
                float num = 2f * Vector3.Angle(MotionControls._rightHandParent.forward, __instance.transform.forward);
                ____focused = (num <= ____viewingWindow);
                var Base = __instance as SingleInteractionVolume;

                var method = typeof(SingleInteractionVolume).GetMethod("UpdateInteractVolume");
                var ftn = method.MethodHandle.GetFunctionPointer();
                var func = (Action)Activator.CreateInstance(typeof(Action), __instance, ftn);

                func();

                return false;
            }

            static void ZoomIn() {
                Common.MainCamera.transform.localScale = Vector3.one * 0.1f;
            }

            static void ZoomOut() {
                Common.MainCamera.transform.localScale = Vector3.one;
            }

            static void SuitUp() {
                MotionControls.ProbeUI.SetValue("_nonSuitUI", false);
            }

            static void RemoveSuit() {
                MotionControls.ProbeUI.SetValue("_nonSuitUI", true);
            }
        }
    }
}
