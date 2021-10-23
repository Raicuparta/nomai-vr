using NomaiVR.Assets;
using NomaiVR.Hands;
using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.Ship
{
    internal class ShipTools : NomaiVRModule<ShipTools.Behaviour, ShipTools.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private ReferenceFrameTracker referenceFrameTracker;
            private static Transform mapGridRenderer;
            private static ShipInteractReceiver probe;
            private static ShipInteractReceiver signalscope;
            private static ShipInteractReceiver landingCam;
            private static ShipInteractReceiver autoPilot;
            public static bool IsLandingCamEnabled; // TODO not public please.

            internal void Awake()
            {
                referenceFrameTracker = FindObjectOfType<ReferenceFrameTracker>();
                mapGridRenderer = FindObjectOfType<MapController>()._gridRenderer.transform;
            }

            internal void Update()
            {
                if (referenceFrameTracker.isActiveAndEnabled && ToolHelper.IsUsingAnyTool())
                {
                    referenceFrameTracker.enabled = false;
                }
                else if (!referenceFrameTracker.isActiveAndEnabled && !ToolHelper.IsUsingAnyTool())
                {
                    referenceFrameTracker.enabled = true;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ShipBody>("Start", nameof(ShipStart));
                    Prefix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInLineOfSight), nameof(PreFindFrame));
                    Postfix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInLineOfSight), nameof(PostFindFrame));
                    Prefix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInMapView), nameof(PreFindFrame));
                    Postfix<ReferenceFrameTracker>(nameof(ReferenceFrameTracker.FindReferenceFrameInMapView), nameof(PostFindFrame));
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
                }

                private static void PreCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr._usingLandingCam = IsLandingCamEnabled;
                }

                private static void PostCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr._usingLandingCam = false;
                }

                private static bool PreEnterLandingView(
                    LandingCamera ____landingCam,
                    ShipLight ____landingLight,
                    ShipCameraComponent ____landingCamComponent,
                    ShipAudioController ____shipAudioController
                )
                {
                    IsLandingCamEnabled = true;
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
                    IsLandingCamEnabled = false;
                    ____landingCam.enabled = false;
                    ____landingLight.SetOn(false);
                    ____shipAudioController.PlayLandingCamOff();

                    return false;
                }

                private static void PostExitFlightConsole(ShipCockpitController __instance)
                {
                    __instance.ExitLandingView();
                }

                private static void ShipStart(ShipBody __instance)
                {
                    var cockpitUI = __instance.transform.Find("Module_Cockpit/Systems_Cockpit/ShipCockpitUI");
                    var cockpitTech = __instance.transform.Find("Module_Cockpit/Geo_Cockpit/Cockpit_Tech/Cockpit_Tech_Interior");

                    probe = cockpitUI.Find("ProbeScreen/ProbeScreenPivot").gameObject.AddComponent<ShipProbeInteract>();
                    signalscope = cockpitUI.Find("SignalScreen/SignalScreenPivot").gameObject.AddComponent<ShipSignalscopeInteract>();
                    landingCam = cockpitTech.Find("LandingCamScreen").gameObject.AddComponent<ShipLandingCamInteract>();
                    
                    var autopilotButton = Instantiate(AssetLoader.AutopilotButtonPrefab, cockpitTech, false);
                    autoPilot = autopilotButton.AddComponent<AutopilotButtonInteract>();
                }

                private static Vector3 cameraPosition;
                private static Quaternion cameraRotation;

                private static void PreFindFrame(ReferenceFrameTracker __instance)
                {
                    if (__instance._isLandingView)
                    {
                        return;
                    }

                    var activeCam = __instance._activeCam.transform;
                    cameraPosition = activeCam.position;
                    cameraRotation = activeCam.rotation;

                    if (__instance._isMapView)
                    {
                        activeCam.position = mapGridRenderer.position + mapGridRenderer.up * 10000;
                        activeCam.rotation = Quaternion.LookRotation(mapGridRenderer.up * -1);
                    }
                    else
                    {
                        activeCam.position = LaserPointer.Behaviour.Laser.position;
                        activeCam.rotation = LaserPointer.Behaviour.Laser.rotation;
                    }
                }

                private static bool IsAnyInteractionFocused()
                {
                    return (probe != null && probe.IsFocused()) || 
                           (signalscope != null && signalscope.IsFocused()) || 
                           (landingCam != null && landingCam.IsFocused()) ||
                           (autoPilot != null && autoPilot.IsFocused());
                }

                private static bool PreUntargetFrame()
                {
                    return !IsAnyInteractionFocused();
                }

                private static ReferenceFrame PostFindFrame(ReferenceFrame __result, ReferenceFrameTracker __instance)
                {
                    if (__instance._isLandingView) return __result;

                    var activeCam = __instance._activeCam.transform;
                    activeCam.position = cameraPosition;
                    activeCam.rotation = cameraRotation;

                    return IsAnyInteractionFocused() ? __instance._currentReferenceFrame : __result;
                }
            }
        }
    }
}
