﻿using OWML.Common;
using OWML.ModHelper.Events;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        protected static Transform RightHand;
        protected static Transform ProbeLauncherModel;
        protected static ProbeLauncherUI ProbeUI;
        protected static Transform SignalscopeReticule;
        protected static Transform ShipWindshield;
        protected static Signalscope SignalScope;
        Transform _leftHandParent;
        Transform _debugTransform;
        Transform _handsWrapper;
        bool _angleMode;
        bool _enableLaser = false;
        bool _handNearHead = false;

        void Start() {
            NomaiVR.Log("Start MotionControls");

            NomaiVR.Helper.Events.Subscribe<Signalscope>(Events.AfterStart);
            NomaiVR.Helper.Events.Subscribe<ShipCockpitUI>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(Signalscope) && ev == Events.AfterStart) {
                _handsWrapper = new GameObject().transform;
                RightHand = CreateHand("PlayerSuit_Glove_Right", SteamVR_Actions.default_RightPose, Quaternion.Euler(45, 180, 0), _handsWrapper);
                _leftHandParent = CreateHand("PlayerSuit_Glove_Left", SteamVR_Actions.default_LeftPose, Quaternion.Euler(-40, 330, 20), _handsWrapper);
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
                HoldSignalscope();
                HoldProbeLauncher();
                HoldTranslator();
                HoldMallow();
                HoldHUD();

                ShipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;

                // Move helmet forward to make it a bit more visible.
                FindObjectOfType<HUDHelmetAnimator>().transform.localPosition += Vector3.forward * 0.2f;

                // For aiming at interactibles with hand:
                //NomaiVR.Helper.HarmonyHelper.AddPrefix<InteractZone>("UpdateInteractVolume", typeof(Patches), "PatchUpdateInteractVolume");

                // For fixing signalscope zoom
                //NomaiVR.Helper.HarmonyHelper.AddPostfix<Signalscope>("EnterSignalscopeZoom", typeof(Patches), "ZoomIn");
                //NomaiVR.Helper.HarmonyHelper.AddPostfix<Signalscope>("ExitSignalscopeZoom", typeof(Patches), "ZoomOut");
                //behaviour.SetValue("_targetFOV", Common.MainCamera.fieldOfView);

                NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerSpacesuit>("SuitUp", typeof(Patches), "SuitUp");
                NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerSpacesuit>("RemoveSuit", typeof(Patches), "RemoveSuit");
                NomaiVR.Helper.HarmonyHelper.AddPrefix<OWInput>("ChangeInputMode", typeof(Patches), "ChangeInputMode");
                NomaiVR.Helper.HarmonyHelper.EmptyMethod<RoastingStickController>("UpdateRotation");
            } else if (behaviour.GetType() == typeof(ShipCockpitUI) && ev == Events.AfterStart) {
                behaviour.SetValue("_signalscopeTool", SignalScope);
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

        void HideArms() {
            var palyerMeshes = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");

            var suitMesh = palyerMeshes.Find("Traveller_Mesh_v01:Traveller_Geo");
            suitMesh.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject.SetActive(false);
            suitMesh.Find("Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject.SetActive(false);
            suitMesh.Find("Traveller_Mesh_v01:Props_HEA_Jetpack").gameObject.SetActive(false);

            var bodyMesh = palyerMeshes.Find("player_mesh_noSuit:Traveller_HEA_Player");
            bodyMesh.Find("player_mesh_noSuit:Player_RightArm").gameObject.SetActive(false);
            bodyMesh.Find("player_mesh_noSuit:Player_LeftArm").gameObject.SetActive(false);
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
            HoldObject(signalScope, RightHand, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));
            SignalScope = signalScope.GetComponent<Signalscope>();

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
            var holster = signalScopeHolster.AddComponent<HolsterTool>();
            holster.hand = RightHand;
            holster.position = new Vector3(0.3f, 0.35f, 0);
            holster.mode = ToolMode.SignalScope;
            holster.scale = 0.8f;
            holster.angle = Vector3.right * 90;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            SignalscopeReticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");
            var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
            var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");

            // Attatch Signalscope UI to the Signalscope.
            SignalscopeReticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            SignalscopeReticule.parent = signalScope;
            SignalscopeReticule.localScale = Vector3.one * 0.0005f;
            SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
            SignalscopeReticule.localRotation = Quaternion.identity;

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

        void HoldProbeLauncher() {
            var probeLauncher = Common.MainCamera.transform.Find("ProbeLauncher");
            probeLauncher.localScale = Vector3.one * 0.2f;
            HoldObject(probeLauncher, RightHand, new Vector3(-0.04f, 0.09f, 0.03f), Quaternion.Euler(45, 0, 0));

            ProbeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
            ProbeLauncherModel.gameObject.layer = 0;
            ProbeLauncherModel.localPosition = Vector3.zero;
            ProbeLauncherModel.localRotation = Quaternion.identity;

            ProbeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);
            ProbeLauncherModel.Find("Props_HEA_Probe_Prelaunch/Props_HEA_Probe_Prelaunch_Prepass").gameObject.SetActive(false);

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
            launchOrigin.parent = ProbeLauncherModel;
            launchOrigin.localPosition = Vector3.forward * 0.2f;
            launchOrigin.localRotation = Quaternion.identity;

            var probeLauncherHolster = Instantiate(ProbeLauncherModel).gameObject;
            probeLauncherHolster.SetActive(true);
            var holster = probeLauncherHolster.AddComponent<HolsterTool>();
            holster.hand = RightHand;
            holster.position = new Vector3(0, 0.35f, 0.2f);
            holster.mode = ToolMode.Probe;
            holster.scale = 0.15f;
            holster.angle = Vector3.right * 90;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            var display = playerHUD.Find("HelmetOffUI/ProbeDisplay");
            display.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            display.parent = ProbeLauncherModel;
            display.localScale = Vector3.one * 0.0012f;
            display.localRotation = Quaternion.identity;
            display.localPosition = Vector3.forward * -0.67f;
            ProbeUI = display.GetComponent<ProbeLauncherUI>();

            var displayImage = display.GetChild(0).GetComponent<RectTransform>();
            displayImage.anchorMin = Vector2.one * 0.5f;
            displayImage.anchorMax = Vector2.one * 0.5f;
            displayImage.pivot = Vector2.one * 0.5f;
            displayImage.localPosition = Vector3.zero;
            displayImage.localRotation = Quaternion.identity;

            playerHUD.Find("HelmetOnUI/UICanvas/HUDProbeDisplay/Image").gameObject.SetActive(false);
        }

        void HoldTranslator() {
            var translator = Common.MainCamera.transform.Find("NomaiTranslatorProp");

            HoldObject(translator, RightHand, new Vector3(-0.24f, 0.08f, 0.06f), Quaternion.Euler(32.8f, 0f, 0f));

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

            var translatorHolster = Instantiate(translatorModel).gameObject;
            translatorHolster.SetActive(true);
            var holster = translatorHolster.AddComponent<HolsterTool>();
            holster.hand = RightHand;
            holster.position = new Vector3(-0.3f, 0.35f, 0);
            holster.mode = ToolMode.Translator;
            holster.scale = 0.15f;
            holster.angle = new Vector3(0, 90, 90);
        }

        void HoldMallow() {
            var scale = Vector3.one * 0.75f;
            var stickController = Locator.GetPlayerBody().transform.Find("RoastingSystem").GetComponent<RoastingStickController>();

            // Move the stick forward while not pressing RT.
            stickController.SetValue("_stickMinZ", 1f);

            var stickRoot = stickController.transform.Find("Stick_Root/Stick_Pivot");
            stickRoot.localScale = scale;
            HoldObject(stickRoot, RightHand, new Vector3(-0.08f, -0.07f, -0.32f));

            var mallow = stickRoot.Find("Stick_Tip/Mallow_Root").GetComponent<Marshmallow>();

            void EatMallow() {
                if (mallow.GetState() != Marshmallow.MallowState.Gone) {
                    mallow.Eat();
                }
            }

            void ReplaceMallow() {
                if (mallow.GetState() == Marshmallow.MallowState.Gone) {
                    mallow.SpawnMallow(true);
                }
            }

            bool ShouldRenderMallowClone() {
                return stickController.enabled && mallow.GetState() == Marshmallow.MallowState.Gone;
            }

            // Eat mallow by moving it to player head.
            var eatDetector = mallow.gameObject.AddComponent<ProximityDetector>();
            eatDetector.other = Common.PlayerHead;
            eatDetector.onEnter += EatMallow;

            // Hide arms that are part of the stick object.
            var meshes = stickRoot.Find("Stick_Tip/Props_HEA_RoastingStick");
            meshes.Find("RoastingStick_Arm").gameObject.SetActive(false);
            meshes.Find("RoastingStick_Arm_NoSuit").gameObject.SetActive(false);

            // Hold mallow in left hand for replacing the one in the stick.
            var mallowModel = mallow.transform.Find("Props_HEA_Marshmallow");
            var mallowClone = Instantiate(mallowModel);
            mallowClone.GetComponent<MeshRenderer>().material.color = Color.white;
            mallowClone.localScale = scale;
            HoldObject(mallowClone, _leftHandParent, new Vector3(0.06f, -0.03f, -0.02f));

            // Replace right hand mallow on proximity with left hand mallow.
            var replaceDetector = mallowClone.gameObject.AddComponent<ProximityDetector>();
            replaceDetector.other = mallow.transform;
            replaceDetector.onEnter += ReplaceMallow;

            // Render left hand mallow only when right hand mallow is not present.
            mallowClone.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderMallowClone;

            var campfires = GameObject.FindObjectsOfType<Campfire>();
            foreach (var campfire in campfires) {
                void StartRoasting() {
                    campfire.Invoke("StartRoasting");
                }
                var stickClone = Instantiate(meshes.Find("RoastingStick_Stick"));
                var stickCloneMallow = Instantiate(mallowModel);
                stickCloneMallow.parent = stickClone;
                stickCloneMallow.localPosition = new Vector3(0, 0, 1.8f);
                stickCloneMallow.localRotation = Quaternion.Euler(145, -85, -83);
                stickClone.gameObject.SetActive(true);
                stickClone.localScale = scale;
                stickClone.parent = campfire.transform;
                stickClone.localPosition = new Vector3(1.44f, 0, .019f);
                stickClone.localRotation = Quaternion.Euler(-100, 125, -125);

                var detector = stickClone.gameObject.AddComponent<ProximityDetector>();
                detector.other = RightHand;
                detector.minDistance = 2;
                detector.onEnter += StartRoasting;
            }

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

        void UpdateFlashlightGesture() {
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

        void UpdateProbeRotation() {
            if (ProbeLauncherModel) {
                var probe = Locator.GetProbe().transform.Find("CameraPivot");
                probe.rotation = ProbeLauncherModel.rotation;
                probe.Rotate(Vector3.right * 90);
            }
        }

        void UpdateHandPosition() {
            if (_handsWrapper) {
                _handsWrapper.localPosition = Common.MainCamera.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
            }
        }

        void UpdateDebugTransform() {
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

        void Update() {
            UpdateHandPosition();
            UpdateProbeRotation();
            UpdateFlashlightGesture();
            UpdateDebugTransform();
        }

        internal static class Patches
        {
            static bool PatchUpdateInteractVolume(
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

            static void ChangeInputMode(InputMode mode) {
                if (!SignalscopeReticule) {
                    return;
                }
                if (mode == InputMode.ShipCockpit || mode == InputMode.LandingCam) {
                    SignalscopeReticule.parent = ShipWindshield;
                    SignalscopeReticule.localScale = Vector3.one * 0.004f;
                    SignalscopeReticule.localPosition = Vector3.forward * 3f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                } else {
                    SignalscopeReticule.parent = SignalScope.transform;
                    SignalscopeReticule.localScale = Vector3.one * 0.0005f;
                    SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
