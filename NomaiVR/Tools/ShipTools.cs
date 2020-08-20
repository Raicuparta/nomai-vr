using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class ShipTools : NomaiVRModule<ShipTools.Behaviour, ShipTools.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private ReferenceFrameTracker _referenceFrameTracker;
            private static Transform _mapGridRenderer;
            private static ShipMonitorInteraction _probe;
            private static ShipMonitorInteraction _signalscope;
            private static ShipMonitorInteraction _landingCam;
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
                if (_referenceFrameTracker.isActiveAndEnabled && ToolHelper.IsUsingAnyTool())
                {
                    _referenceFrameTracker.enabled = false;
                }
                else if (!_referenceFrameTracker.isActiveAndEnabled && !ToolHelper.IsUsingAnyTool())
                {
                    _referenceFrameTracker.enabled = true;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ShipBody>("Start", nameof(ShipStart));
                    Prefix<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", nameof(PreFindFrame));
                    Postfix<ReferenceFrameTracker>("FindReferenceFrameInLineOfSight", nameof(PostFindFrame));
                    Prefix<ReferenceFrameTracker>("FindReferenceFrameInMapView", nameof(PreFindFrame));
                    Postfix<ReferenceFrameTracker>("FindReferenceFrameInMapView", nameof(PostFindFrame));
                    Empty<PlayerCameraController>("OnEnterLandingView");
                    Empty<PlayerCameraController>("OnExitLandingView");
                    Empty<PlayerCameraController>("OnEnterShipComputer");
                    Empty<PlayerCameraController>("OnExitShipComputer");
                    Prefix<ShipCockpitController>("EnterLandingView", nameof(PreEnterLandingView));
                    Prefix<ShipCockpitController>("ExitLandingView", nameof(PreExitLandingView));
                    Postfix<ShipCockpitController>("ExitFlightConsole", nameof(PostExitFlightConsole));
                    Prefix<ShipCockpitUI>("Update", nameof(PreCockpitUIUpdate));
                    Postfix<ShipCockpitUI>("Update", nameof(PostCockpitUIUpdate));
                    Prefix(typeof(ReferenceFrameTracker).GetMethod("UntargetReferenceFrame", new[] { typeof(bool) }), nameof(PreUntargetFrame));
                    Prefix<ProbeLauncher>("RetrieveProbe", nameof(PreRetrieveProbe));
                }

                private static bool PreRetrieveProbe()
                {
                    if (Locator.GetReferenceFrame(true) != null && OWInput.IsInputMode(InputMode.ShipCockpit))
                    {
                        return false;
                    }
                    return true;
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

                private static bool ShouldRenderScreenText()
                {
                    return Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None);
                }

                private static void ShipStart(ShipBody __instance)
                {
                    var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");

                    var probeScreenPivot = cockpitUI.Find("ProbeScreen/ProbeScreenPivot");
                    _probe = probeScreenPivot.Find("ProbeScreen").gameObject.AddComponent<ShipMonitorInteraction>();
                    _probe.mode = ToolMode.Probe;
                    _probe.text = UITextType.ScoutModePrompt;

                    var font = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular");

                    var probeCamDisplay = probeScreenPivot.Find("ProbeCamDisplay");
                    var probeScreenText = new GameObject().AddComponent<Text>();
                    probeScreenText.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = ShouldRenderScreenText;
                    probeScreenText.transform.SetParent(probeCamDisplay.transform, false);
                    probeScreenText.transform.localScale = Vector3.one * 0.0035f;
                    probeScreenText.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    probeScreenText.text = "<color=grey>PROBE LAUNCHER</color>\n\ninteract with screen\nto activate";
                    probeScreenText.color = new Color(1, 1, 1, 0.1f);
                    probeScreenText.alignment = TextAnchor.MiddleCenter;
                    probeScreenText.fontSize = 8;
                    probeScreenText.font = font;

                    var signalScreenPivot = cockpitUI.Find("SignalScreen/SignalScreenPivot");
                    _signalscope = signalScreenPivot.Find("SignalScopeScreenFrame_geo").gameObject.AddComponent<ShipMonitorInteraction>();
                    _signalscope.mode = ToolMode.SignalScope;
                    _signalscope.text = UITextType.UISignalscope;

                    var sigScopeDisplay = signalScreenPivot.Find("SigScopeDisplay");
                    var scopeTextCanvas = new GameObject().AddComponent<Canvas>();
                    scopeTextCanvas.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = ShouldRenderScreenText;
                    scopeTextCanvas.transform.SetParent(sigScopeDisplay.transform.parent, false);
                    scopeTextCanvas.transform.localPosition = sigScopeDisplay.transform.localPosition;
                    scopeTextCanvas.transform.localRotation = sigScopeDisplay.transform.localRotation;
                    scopeTextCanvas.transform.localScale = sigScopeDisplay.transform.localScale;
                    var scopeScreenText = new GameObject().AddComponent<Text>();
                    scopeScreenText.transform.SetParent(scopeTextCanvas.transform, false);
                    scopeScreenText.transform.localScale = Vector3.one * 0.5f;
                    scopeScreenText.text = "<color=grey>SIGNALSCOPE</color>\n\ninteract with screen to activate";
                    scopeScreenText.color = new Color(1, 1, 1, 0.1f);
                    scopeScreenText.alignment = TextAnchor.MiddleCenter;
                    scopeScreenText.fontSize = 8;
                    scopeScreenText.font = font;

                    var cockpitTech = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");

                    _landingCam = cockpitTech.Find("LandingCamScreen").gameObject.AddComponent<ShipMonitorInteraction>();
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

                    var landingTextCanvas = new GameObject().AddComponent<Canvas>();
                    landingTextCanvas.transform.SetParent(_landingCam.transform.parent, false);
                    landingTextCanvas.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () => ShouldRenderScreenText() && !_isLandingCamEnabled;
                    landingTextCanvas.transform.localPosition = new Vector3(-0.017f, 3.731f, 5.219f);
                    landingTextCanvas.transform.localRotation = Quaternion.Euler(53.28f, 0, 0);
                    landingTextCanvas.transform.localScale = Vector3.one * 0.007f;
                    var landingText = new GameObject().AddComponent<Text>();
                    landingText.transform.SetParent(landingTextCanvas.transform, false);
                    landingText.transform.localScale = Vector3.one * 0.6f;
                    landingText.text = "<color=grey>LANDING CAMERA</color>\n\ninteract with screen\nto activate";
                    landingText.color = new Color(1, 1, 1, 0.1f);
                    landingText.alignment = TextAnchor.MiddleCenter;
                    landingText.fontSize = 8;
                    landingText.font = font;
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

                private static bool IsAnyInteractionFocused()
                {
                    return _probe.IsFocused() || _signalscope.IsFocused() || _landingCam.IsFocused();
                }

                private static bool PreUntargetFrame()
                {
                    return !IsAnyInteractionFocused();
                }

                private static ReferenceFrame PostFindFrame(
                    ReferenceFrame __result,
                    OWCamera ____activeCam,
                    ReferenceFrame ____currentReferenceFrame,
                    bool ____isLandingView
                )
                {
                    if (____isLandingView)
                    {
                        return __result;
                    }

                    ____activeCam.transform.position = _cameraPosition;
                    ____activeCam.transform.rotation = _cameraRotation;

                    if (IsAnyInteractionFocused())
                    {
                        return ____currentReferenceFrame;
                    }

                    return __result;
                }
            }
        }
    }
}
