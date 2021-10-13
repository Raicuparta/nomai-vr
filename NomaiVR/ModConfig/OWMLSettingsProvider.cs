using OWML.Common;
using System;

namespace NomaiVR.ModConfig
{
    public class OWMLSettingsProvider : IModSettingProvider
    {
        public event Action OnConfigChange;

        public bool LeftHandDominant { get; private set; } = false;
        public bool DebugMode { get; private set; } = true;
        public bool PreventCursorLock { get; private set; }
        public bool ShowHelmet { get; private set; }
        public int OverrideRefreshRate { get; private set; }
        public float VibrationStrength { get; private set; }
        public bool EnableGesturePrompts { get; private set; }
        public bool EnableHandLaser { get; private set; }
        public bool EnableFeetMarker { get; private set; }
        public bool ControllerOrientedMovement { get; private set; }
        public bool AutoHideToolbelt { get; private set; }
        public bool BypassFatalErrors { get; private set; }
        public float ToolbeltHeight { get; private set; }
        public float HudScale { get; private set; }
        public float HudOpacity { get; private set; }

        private IModConfig config;
        public OWMLSettingsProvider(IModConfig config)
        {
            this.config = config;
        }

        public void Configure()
        {
            LeftHandDominant = config.GetSettingsValue<bool>("leftHandDominant");
            OverrideRefreshRate = config.GetSettingsValue<int>("refreshRateOverride");
            VibrationStrength = config.GetSettingsValue<float>("vibrationIntensity");
            ShowHelmet = config.GetSettingsValue<bool>("helmetVisibility");
            ControllerOrientedMovement = config.GetSettingsValue<bool>("movementControllerOriented");
            EnableGesturePrompts = config.GetSettingsValue<bool>("showGesturePrompts");
            EnableHandLaser = config.GetSettingsValue<bool>("showHandLaser");
            EnableFeetMarker = config.GetSettingsValue<bool>("showFeetMarker");
            PreventCursorLock = config.GetSettingsValue<bool>("disableCursorLock");
            DebugMode = config.GetSettingsValue<bool>("debug");
            AutoHideToolbelt = config.GetSettingsValue<bool>("autoHideToolbelt");
            HudScale = config.GetSettingsValue<float>("hudScale");
            HudOpacity = config.GetSettingsValue<float>("hudOpacity");
            BypassFatalErrors = config.GetSettingsValue<bool>("bypassFatalErrors");

            // OWML doesn't support negative slider values so I subtract it here.
            ToolbeltHeight = config.GetSettingsValue<float>("toolbeltHeight") - 1f;

            OnConfigChange?.Invoke();
        }
    }
}
