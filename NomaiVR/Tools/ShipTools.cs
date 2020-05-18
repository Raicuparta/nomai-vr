using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class ShipTools : NomaiVRModule<ShipTools.Behaviour, ShipTools.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            bool _wasHoldingInteract;
            bool _pressedInteract;
            ReferenceFrameTracker _referenceFrameTracker;
            static Transform _mapGridRenderer;
            static ButtonInteraction _probe;
            static ButtonInteraction _signalscope;
            static ButtonInteraction _landingCam;
            static bool _canInteractWithTools;
            static ShipCockpitController _cockpitController;
            static bool _isLandingCamEnabled;

            void Awake()
            {
                _referenceFrameTracker = FindObjectOfType<ReferenceFrameTracker>();
                _cockpitController = FindObjectOfType<ShipCockpitController>();
                _mapGridRenderer = FindObjectOfType<MapController>().GetValue<MeshRenderer>("_gridRenderer").transform;
            }

            void Update()
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
                    ControllerInput.Behaviour.SimulateInput(XboxAxis.dPadY, 1);
                    _wasHoldingInteract = true;
                }
                if (OWInput.IsNewlyReleased(InputLibrary.interact))
                {
                    if (_wasHoldingInteract)
                    {
                        ControllerInput.Behaviour.SimulateInput(XboxAxis.dPadY, 0);
                        _wasHoldingInteract = false;
                    }
                    else if (_pressedInteract && !IsFocused(_probe) && !IsFocused(_signalscope) && !IsFocused(_landingCam))
                    {
                        ControllerInput.Behaviour.SimulateInput(XboxButton.LeftStickClick);
                    }
                    _pressedInteract = false;
                }
            }

            bool IsFocused(ButtonInteraction interaction)
            {
                return interaction && interaction.receiver && interaction.receiver.IsFocused();
            }

            static void SetEnabled(bool enabled)
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
                    NomaiVR.Post<ShipBody>("Start", typeof(Patch), nameof(ShipStart));
                    NomaiVR.Pre<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(Patch), nameof(PreFindFrame));
                    NomaiVR.Post<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", typeof(Patch), nameof(PostFindFrame));
                    NomaiVR.Pre<ReferenceFrameTracker>("FindReferenceFrameInMapView", typeof(Patch), nameof(PreFindFrame));
                    NomaiVR.Post<ReferenceFrameTracker>("FindReferenceFrameInMapView", typeof(Patch), nameof(PostFindFrame));
                    NomaiVR.Empty<PlayerCameraController>("OnEnterLandingView");
                    NomaiVR.Empty<PlayerCameraController>("OnExitLandingView");
                    NomaiVR.Empty<PlayerCameraController>("OnEnterShipComputer");
                    NomaiVR.Empty<PlayerCameraController>("OnExitShipComputer");

                    NomaiVR.Pre<ShipCockpitController>("EnterLandingView", typeof(Patch), nameof(PreEnterLandingView));
                    NomaiVR.Pre<ShipCockpitController>("ExitLandingView", typeof(Patch), nameof(PreExitLandingView));
                    NomaiVR.Pre<ShipCockpitUI>("Update", typeof(Patch), nameof(PreCockpitUIUpdate));
                    NomaiVR.Post<ShipCockpitUI>("Update", typeof(Patch), nameof(PostCockpitUIUpdate));
                }

                static void PreCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr.SetValue("_usingLandingCam", _isLandingCamEnabled);
                }

                static void PostCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr.SetValue("_usingLandingCam", false);
                }

                static bool PreEnterLandingView(
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

                static bool PreExitLandingView(
                    LandingCamera ____landingCam,
                    ShipLight ____landingLight,
                    ShipCameraComponent ____landingCamComponent,
                    ShipAudioController ____shipAudioController
                )
                {
                    _isLandingCamEnabled = false;
                    ____landingCam.enabled = false;
                    ____landingLight.SetOn(false);
                    ____shipAudioController.PlayLandingCamOff();

                    return false;
                }

                static void ShipStart(ShipBody __instance)
                {
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

                static Vector3 _cameraPosition;
                static Quaternion _cameraRotation;

                static void PreFindFrame(
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

                static void PostFindFrame(
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
