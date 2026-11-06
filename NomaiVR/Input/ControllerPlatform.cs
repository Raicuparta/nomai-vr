using System;
using Valve.VR;

namespace NomaiVR.Input
{
    public enum ActiveVRPlatform
    {
        Oculus, //G2 and oculus style controllers should be bound here
        Index,
        Vive,
        Wmr,
        SteamFrame,
        Generic
    }


    public static class ControllerPlatform
    {
        public static ActiveVRPlatform Current { get; private set; } = ActiveVRPlatform.Generic;
        public static event Action Changed;
        private static bool isInitialized;
        public static void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            SteamVR_Actions._default.Grip.AddOnActiveBindingChangeListener((fromAction, fromSource, active) => Detect(fromAction), SteamVR_Input_Sources.Any);
            Detect(SteamVR_Actions._default.Grip);
        }

        private static void Detect(SteamVR_Action_Boolean fromAction)
        {
            if (!fromAction.active || fromAction.activeOrigin == 0) return;

            var activeDevice = SteamVR.instance.GetStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, fromAction.trackedDeviceIndex);
            Logs.Write($"#### Got active device: {activeDevice}");

            ActiveVRPlatform platform;
            switch (activeDevice)
            {
                case "oculus_touch":
                    platform = ActiveVRPlatform.Oculus;
                    break;
                case "vive_controller":
                    platform = ActiveVRPlatform.Vive;
                    break;
                case "knuckles":
                    platform = ActiveVRPlatform.Index;
                    break;
                case "holographic_controller":
                    platform = ActiveVRPlatform.Wmr;
                    break;
                case "frame_controller":
                    platform = ActiveVRPlatform.SteamFrame;
                    break;
                case "<unknown>":
                    return;
                default:
                    platform = ActiveVRPlatform.Generic;
                    break;
            }

            Logs.Write($"Controller platform: {platform}");
            if (platform == Current) return;

            Current = platform;
            Changed?.Invoke();
        }
    }
}
