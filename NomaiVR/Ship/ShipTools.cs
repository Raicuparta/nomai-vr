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

                private static void PreCockpitUIUpdate(ShipCockpitUI __instance)
                {
                    __instance._shipSystemsCtrlr._usingLandingCam = __instance._shipSystemsCtrlr._landingCam.enabled;
                }

                private static void PostCockpitUIUpdate(ShipCockpitController ____shipSystemsCtrlr)
                {
                    ____shipSystemsCtrlr._usingLandingCam = false;
                }

                private static bool PreEnterLandingView(ShipCockpitController __instance)
                {
                    __instance._landingCam.enabled = true;
                    __instance._landingLight.SetOn(true);

                    __instance._shipAudioController.PlayLandingCamOn(__instance._landingCamComponent.isDamaged
                        ? AudioType.ShipCockpitLandingCamStatic_LP
                        : AudioType.ShipCockpitLandingCamAmbient_LP);

                    return false;
                }

                private static bool PreExitLandingView(ShipCockpitController __instance)
                {
                    __instance._landingCam.enabled = false;
                    __instance._landingLight.SetOn(false);
                    __instance._shipAudioController.PlayLandingCamOff();

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
                        var up = mapGridRenderer.up;
                        activeCam.position = mapGridRenderer.position + up * 10000;
                        activeCam.rotation = Quaternion.LookRotation(up * -1);
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
