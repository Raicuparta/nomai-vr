using OWML.Common;
using System;

namespace NomaiVR.ModConfig
{
    public class OwmlSettingsProvider : IModSettingProvider
    {
        public event Action OnConfigChange;

        public bool LeftHandDominant { get; private set; }
        public bool DebugMode { get; private set; } = true;
        public bool PreventCursorLock { get; private set; }
        public bool ShowHelmet { get; private set; }
        public float VibrationStrength { get; private set; }
        public bool EnableGesturePrompts { get; private set; }
        public bool EnableHandLaser { get; private set; }
        public bool EnableFeetMarker { get; private set; }
        public bool PreventClipping { get; private set; }
        public bool FlashlightGesture { get; private set; }
        public bool ControllerOrientedMovement { get; private set; }
        public bool SnapTurning { get; private set; }
        public float SnapTurnIncrement { get; private set; }
        public bool AutoHideToolbelt { get; private set; }
        public float ToolbeltHeight { get; private set; }
        public float HudScale { get; private set; }
        public float HudOpacity { get; private set; }
        public float MarkersOpacity { get; private set; }
        public float LookArrowOpacity { get; private set; }
        public bool HudSmoothFollow { get; private set; } = true;

        private readonly IModConfig config;
        public OwmlSettingsProvider(IModConfig config)
        {
            this.config = config;
        }

        public void Configure()
        {
            LeftHandDominant = config.GetSettingsValue<bool>("leftHandDominant");
            VibrationStrength = config.GetSettingsValue<float>("vibrationIntensity");
            ShowHelmet = config.GetSettingsValue<bool>("helmetVisibility");
            ControllerOrientedMovement = config.GetSettingsValue<bool>("movementControllerOriented");
            SnapTurning = config.GetSettingsValue<bool>("snapTurning");
            SnapTurnIncrement = config.GetSettingsValue<float>("snapTurnIncrement");
            EnableGesturePrompts = config.GetSettingsValue<bool>("showGesturePrompts");
            EnableHandLaser = config.GetSettingsValue<bool>("showHandLaser");
            EnableFeetMarker = config.GetSettingsValue<bool>("showFeetMarker");
            FlashlightGesture = config.GetSettingsValue<bool>("flashlightGesture");
            PreventClipping = config.GetSettingsValue<bool>("preventClipping");
            DebugMode = config.GetSettingsValue<bool>("debug");
            AutoHideToolbelt = config.GetSettingsValue<bool>("autoHideToolbelt");
            HudScale = config.GetSettingsValue<float>("hudScale");
            HudSmoothFollow = config.GetSettingsValue<bool>("hudSmoothFollow");
            PreventCursorLock = config.GetSettingsValue<bool>("disableCursorLock");
            HudOpacity = config.GetSettingsValue<float>("hudOpacity");
            MarkersOpacity = config.GetSettingsValue<float>("markersOpacity");
            LookArrowOpacity = config.GetSettingsValue<float>("lookArrowOpacity");

            // OWML doesn't support negative slider values so I subtract it here.
            ToolbeltHeight = config.GetSettingsValue<float>("toolbeltHeight") - 1f;

            
            OnConfigChange?.Invoke();
        }
    }
}
