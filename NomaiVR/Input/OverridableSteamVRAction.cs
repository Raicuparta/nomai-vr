using Valve.VR;

namespace NomaiVR
{
    public class OverridableSteamVRAction : ISteamVR_Action_In
    {
        private ISteamVR_Action_In _defaultAction;
        private ISteamVR_Action_In _overrideAction;

        public ISteamVR_Action_In ActiveAction => _overrideAction.active ? _overrideAction : _defaultAction;

        public bool changed => ActiveAction.changed;
        public bool lastChanged => ActiveAction.lastChanged;
        public float changedTime => ActiveAction.changedTime;
        public float updateTime => ActiveAction.updateTime;
        public ulong activeOrigin => ActiveAction.activeOrigin;
        public ulong lastActiveOrigin => ActiveAction.lastActiveOrigin;
        public SteamVR_Input_Sources activeDevice => ActiveAction.activeDevice;
        public uint trackedDeviceIndex => ActiveAction.trackedDeviceIndex;
        public string renderModelComponentName => ActiveAction.renderModelComponentName;
        public string localizedOriginName => ActiveAction.localizedOriginName;
        public bool active => ActiveAction.active;
        public bool activeBinding => ActiveAction.activeBinding;
        public bool lastActive => ActiveAction.lastActive;
        public bool lastActiveBinding => ActiveAction.lastActiveBinding;
        public string fullPath => ActiveAction.fullPath;
        public ulong handle => ActiveAction.handle;
        public SteamVR_ActionSet actionSet => ActiveAction.actionSet;
        public SteamVR_ActionDirections direction => ActiveAction.direction;

        public OverridableSteamVRAction(ISteamVR_Action_In defaultAction, ISteamVR_Action_In overrideAction)
        {
            _defaultAction = defaultAction;
            _overrideAction = overrideAction;
        }

        public void UpdateValues() => ActiveAction.UpdateValues();
        public string GetRenderModelComponentName(SteamVR_Input_Sources inputSource) => ActiveAction.GetRenderModelComponentName(inputSource);
        public SteamVR_Input_Sources GetActiveDevice(SteamVR_Input_Sources inputSource) => ActiveAction.GetActiveDevice(inputSource);
        public uint GetDeviceIndex(SteamVR_Input_Sources inputSource) => ActiveAction.GetDeviceIndex(inputSource);
        public bool GetChanged(SteamVR_Input_Sources inputSource) => ActiveAction.GetChanged(inputSource);
        public string GetLocalizedOriginPart(SteamVR_Input_Sources inputSource, params EVRInputStringBits[] localizedParts) => ActiveAction.GetLocalizedOriginPart(inputSource, localizedParts);
        public string GetLocalizedOrigin(SteamVR_Input_Sources inputSource) => ActiveAction.GetLocalizedOrigin(inputSource);
        public bool GetActive(SteamVR_Input_Sources inputSource) => ActiveAction.GetActive(inputSource);
        public string GetShortName() => ActiveAction.GetShortName();
    }
}
