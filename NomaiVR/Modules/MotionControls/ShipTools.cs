using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    class ShipTools: MonoBehaviour {
        bool _wasHoldingInteract;
        bool _pressedInteract;
        ReferenceFrameTracker _referenceFrameTracker;
        static Transform _mapGridRenderer;
        static ButtonInteraction _probe;
        static ButtonInteraction _signalscope;
        static ButtonInteraction _landingCam;

        void Awake () {
            NomaiVR.Log("Start Ship Tools");

            _referenceFrameTracker = FindObjectOfType<ReferenceFrameTracker>();
            _mapGridRenderer = GameObject.FindObjectOfType<MapController>().GetValue<MeshRenderer>("_gridRenderer").transform;
        }

        void Update () {
            if (_referenceFrameTracker.GetReferenceFrame() == null && _referenceFrameTracker.GetPossibleReferenceFrame() == null) {
                return;
            }
            if (OWInput.IsNewlyPressed(InputLibrary.interact)) {
                _pressedInteract = true;
            }
            if (OWInput.IsNewlyHeld(InputLibrary.interact)) {
                ControllerInput.SimulateInput(XboxAxis.dPadY, 1);
                _wasHoldingInteract = true;
            }
            if (OWInput.IsNewlyReleased(InputLibrary.interact)) {
                if (_wasHoldingInteract) {
                    ControllerInput.SimulateInput(XboxAxis.dPadY, 0);
                    _wasHoldingInteract = false;
                } else if (_pressedInteract && !_probe.receiver.IsFocused() && !_signalscope.receiver.IsFocused() && !_landingCam.receiver.IsFocused()) {
                    ControllerInput.SimulateInput(XboxButton.LeftStickClick);
                }
                _pressedInteract = false;
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ShipBody>("Start", typeof(Patches), nameof(Patches.ShipStart));
                NomaiVR.Pre<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(Patches), nameof(PreFindFrame));
                NomaiVR.Post<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(Patches), nameof(PostFindFrame));
                NomaiVR.Pre<ReferenceFrameTracker>("FindReferenceFrameInMapView", typeof(Patches), nameof(PreFindFrame));
                NomaiVR.Post<ReferenceFrameTracker>("FindReferenceFrameInMapView", typeof(Patches), nameof(PostFindFrame));
            }

            static void ShipStart (ShipBody __instance) {
                var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");

                _probe = cockpitUI.Find("ProbeScreen/ProbeScreenPivot/ProbeScreen").gameObject.AddComponent<ButtonInteraction>();
                _probe.button = XboxButton.RightBumper;
                _probe.text = UITextType.ScoutModePrompt;

                _signalscope = cockpitUI.Find("SignalScreen/SignalScreenPivot/SignalScopeScreenFrame_geo").gameObject.AddComponent<ButtonInteraction>();
                _signalscope.button = XboxButton.DPadRight;
                _signalscope.text = UITextType.SignalscopePrompt;

                var cockpitTech = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");

                _landingCam = cockpitTech.Find("LandingCamScreen").gameObject.AddComponent<ButtonInteraction>();
                _landingCam.button = XboxButton.DPadDown;
                _landingCam.text = UITextType.ShipLandingPrompt;
            }


            static Vector3 _cameraPosition;
            static Quaternion _cameraRotation;

            static void PreFindFrame (
                OWCamera ____activeCam,
                OWCamera ____landingCam,
                bool ____isLandingView,
                bool ____isMapView
            ) {
                var camera = ____isLandingView ? ____landingCam : ____activeCam;
                _cameraPosition = camera.transform.position;
                _cameraRotation = camera.transform.rotation;

                if (____isMapView) {
                    camera.transform.position = _mapGridRenderer.position + _mapGridRenderer.up * 10000;
                    camera.transform.rotation = Quaternion.LookRotation(_mapGridRenderer.up * -1);
                } else {
                    camera.transform.position = LaserPointer.Laser.position;
                    camera.transform.rotation = LaserPointer.Laser.rotation;
                }
            }

            static void PostFindFrame (
                OWCamera ____activeCam,
                OWCamera ____landingCam,
                bool ____isLandingView,
                bool ____isMapView
            ) {
                var camera = ____isLandingView ? ____landingCam : ____activeCam;

                camera.transform.position = _cameraPosition;
                camera.transform.rotation = _cameraRotation;
            }
        }
    }
}
