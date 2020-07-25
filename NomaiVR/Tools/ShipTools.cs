using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    internal class ShipTools : NomaiVRModule<ShipTools.Behaviour, ShipTools.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private bool _wasHoldingInteract;
            private bool _pressedInteract;
            private ReferenceFrameTracker _referenceFrameTracker;
            private static Transform _mapGridRenderer;
            private static ButtonInteraction _probe;
            private static ButtonInteraction _signalscope;
            private static ButtonInteraction _landingCam;
            private static bool _canInteractWithTools;
            private static ShipCockpitController _cockpitController;
            private static bool _isLandingCamEnabled;

            internal void Awake()
            {
                _referenceFrameTracker = FindObjectOfType<ReferenceFrameTracker>();
                _cockpitController = FindObjectOfType<ShipCockpitController>();
                _mapGridRenderer = FindObjectOfType<MapController>().GetValue<MeshRenderer>("_gridRenderer").transform;
            }

            internal void Update()
            {
                var isInShip = _cockpitController.IsPlayerAtFlightConsole();

                if (isInShip && !_canInteractWithTools)
                {
                    SetEnabled(true);
                }
                else if (!isInShip && _canInteractWithTools)
                {
                    SetEnabled(false);
                }

                if (_referenceFrameTracker.isActiveAndEnabled && ToolHelper.IsUsingAnyTool())
                {
                    _referenceFrameTracker.enabled = false;
                }
                else if (!_referenceFrameTracker.isActiveAndEnabled && !ToolHelper.IsUsingAnyTool())
                {
                    _referenceFrameTracker.enabled = true;
                }

                if (_referenceFrameTracker.GetReferenceFrame() == null && _referenceFrameTracker.GetPossibleReferenceFrame() == null)
                {
                    return;
                }
                if (OWInput.IsNewlyPressed(InputLibrary.interact))
                {
                    _pressedInteract = true;
                }
                if (OWInput.IsNewlyHeld(InputLibrary.interact))
                {
                    ControllerInput.Behaviour.SimulateInput(AxisIdentifier.CTRLR_DPADY, 1);
                    _wasHoldingInteract = true;
                }
                if (OWInput.IsNewlyReleased(InputLibrary.interact))
                {
                    if (_wasHoldingInteract)
                    {
                        ControllerInput.Behaviour.SimulateInput(AxisIdentifier.CTRLR_DPADY, 0);
                        _wasHoldingInteract = false;
                    }
                    else if (_pressedInteract && !IsFocused(_probe) && !IsFocused(_signalscope) && !IsFocused(_landingCam))
                    {
                        ControllerInput.Behaviour.SimulateInput(JoystickButton.LeftStickClick);
                    }
                    _pressedInteract = false;
                }
            }

            private static bool IsFocused(ButtonInteraction interaction)
            {
                return interaction && interaction.receiver && interaction.receiver.IsFocused();
            }

            private static void SetEnabled(bool enabled)
            {
                _canInteractWithTools = enabled;
                _probe.enabled = enabled;
                _signalscope.enabled = enabled;
                _landingCam.enabled = enabled;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Post<ShipBody>("Start", nameof(ShipStart));
                    Pre<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", nameof(PreFindFrame));
                    Post<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", nameof(PostFindFrame));
                    Pre<ReferenceFrameTracker>("FindReferenceFrameInMapView", nameof(PreFindFrame));
                    Post<ReferenceFrameTracker>("FindReferenceFrameInMapView", nameof(PostFindFrame));
                    Empty<PlayerCameraController>("OnEnterLandingView");
                    Empty<PlayerCameraController>("OnExitLandingView");
                    Empty<PlayerCameraController>("OnEnterShipComputer");
                    Empty<PlayerCameraController>("OnExitShipComputer");

                    Pre<ShipCockpitController>("EnterLandingView", nameof(PreEnterLandingView));
                    Pre<ShipCockpitController>("ExitLandingView", nameof(PreExitLandingView));
                    Post<ShipCockpitController>("ExitFlightConsole", nameof(PostExitFlightConsole));
                    Pre<ShipCockpitUI>("Update", nameof(PreCockpitUIUpdate));
                    Post<ShipCockpitUI>("Update", nameof(PostCockpitUIUpdate));
                }

                private static void PreCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr.SetValue("_usingLandingCam", _isLandingCamEnabled);
                }

                private static void PostCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr.SetValue("_usingLandingCam", false);
                }

                private static bool PreEnterLandingView(
                    LandingCamera ____landingCam,
                    ShipLight ____landingLight,
                    ShipCameraComponent ____landingCamComponent,
                    ShipAudioController ____shipAudioController
                )
                {
                    _isLandingCamEnabled = true;
                    ____landingCam.enabled = true;
                    ____landingLight.SetOn(true);

                    if (____landingCamComponent.isDamaged)
                    {
                        ____shipAudioController.PlayLandingCamOn(AudioType.ShipCockpitLandingCamStatic_LP);
                    }
                    else
                    {
                        ____shipAudioController.PlayLandingCamOn(AudioType.ShipCockpitLandingCamAmbient_LP);
                    }

                    return false;
                }

                private static bool PreExitLandingView(
                    LandingCamera ____landingCam,
                    ShipLight ____landingLight,
                    ShipAudioController ____shipAudioController
                )
                {
                    _isLandingCamEnabled = false;
                    ____landingCam.enabled = false;
                    ____landingLight.SetOn(false);
                    ____shipAudioController.PlayLandingCamOff();

                    return false;
                }

                private static void PostExitFlightConsole(ShipCockpitController __instance)
                {
                    __instance.Invoke("ExitLandingView");
                }

                private static void ShipStart(ShipBody __instance)
                {
                    var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");

                    _probe = cockpitUI.Find("ProbeScreen/ProbeScreenPivot/ProbeScreen").gameObject.AddComponent<ButtonInteraction>();
                    _probe.button = JoystickButton.RightBumper;
                    _probe.text = UITextType.ScoutModePrompt;

                    _signalscope = cockpitUI.Find("SignalScreen/SignalScreenPivot/SignalScopeScreenFrame_geo").gameObject.AddComponent<ButtonInteraction>();
                    _signalscope.button = JoystickButton.DPadRight;
                    _signalscope.text = UITextType.SignalscopePrompt;

                    var cockpitTech = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");

                    _landingCam = cockpitTech.Find("LandingCamScreen").gameObject.AddComponent<ButtonInteraction>();
                    _landingCam.button = JoystickButton.DPadDown;
                    _landingCam.skipPressCallback = () =>
                    {
                        if (_isLandingCamEnabled)
                        {
                            _cockpitController.Invoke("ExitLandingView");
                            return true;
                        }
                        return false;
                    };
                    _landingCam.text = UITextType.ShipLandingPrompt;

                    SetEnabled(false);
                }

                private static Vector3 _cameraPosition;
                private static Quaternion _cameraRotation;

                private static void PreFindFrame(
                    OWCamera ____activeCam,
                    bool ____isLandingView,
                    bool ____isMapView
                )
                {
                    if (____isLandingView)
                    {
                        return;
                    }

                    _cameraPosition = ____activeCam.transform.position;
                    _cameraRotation = ____activeCam.transform.rotation;

                    if (____isMapView)
                    {
                        ____activeCam.transform.position = _mapGridRenderer.position + _mapGridRenderer.up * 10000;
                        ____activeCam.transform.rotation = Quaternion.LookRotation(_mapGridRenderer.up * -1);
                    }
                    else
                    {
                        ____activeCam.transform.position = LaserPointer.Behaviour.Laser.position;
                        ____activeCam.transform.rotation = LaserPointer.Behaviour.Laser.rotation;
                    }
                }

                private static void PostFindFrame(
                    OWCamera ____activeCam,
                    bool ____isLandingView
                )
                {
                    if (____isLandingView)
                    {
                        return;
                    }

                    ____activeCam.transform.position = _cameraPosition;
                    ____activeCam.transform.rotation = _cameraRotation;
                }
            }
        }
    }
}
