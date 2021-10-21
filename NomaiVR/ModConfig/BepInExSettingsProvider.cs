#if !OWML
using BepInEx.Configuration;
using System;

namespace NomaiVR.ModConfig
{
    public class BepInExSettingsProvider : IModSettingProvider
    {
        public event Action OnConfigChange;

        private ConfigEntry<bool> leftHandDominant;
        public bool LeftHandDominant => leftHandDominant.Value;

        private ConfigEntry<bool> debugMode;
        public bool DebugMode => debugMode.Value;

        private ConfigEntry<bool> preventCursorLock;
        public bool PreventCursorLock => preventCursorLock.Value;

        private ConfigEntry<bool> showHelmet;
        public bool ShowHelmet => showHelmet.Value;

        private ConfigEntry<float> vibrationStrength;
        public float VibrationStrength => vibrationStrength.Value;

        private ConfigEntry<bool> enableGesturePrompts;
        public bool EnableGesturePrompts => enableGesturePrompts.Value;

        private ConfigEntry<bool> enableHandLaser;
        public bool EnableHandLaser => enableHandLaser.Value;

        private ConfigEntry<bool> enableFeetMarker;
        public bool EnableFeetMarker => enableFeetMarker.Value;

        private ConfigEntry<bool> controllerOrientedMovement;
        public bool ControllerOrientedMovement => controllerOrientedMovement.Value;

        private ConfigEntry<bool> autoHideToolbelt;
        public bool AutoHideToolbelt => autoHideToolbelt.Value;

        private ConfigEntry<bool> bypassFatalErrors;
        public bool BypassFatalErrors => bypassFatalErrors.Value;

        private ConfigEntry<float> toolbeltHeight;
        public float ToolbeltHeight => toolbeltHeight.Value;

        private ConfigEntry<float> hudScale;
        public float HudScale => hudScale.Value;

        private ConfigEntry<float> hudOpacity;
        public float HudOpacity => hudOpacity.Value;

        private ConfigFile config;
        public BepInExSettingsProvider(ConfigFile config)
        {
            this.config = config;
        }

        public void Configure()
        {
            // TODO: setting descriptions.
            const string section = "NomaiVRSettings";
            leftHandDominant = config.Bind(section, "leftHandDominant", false, "");
            vibrationStrength = config.Bind(section, "vibrationIntensity", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3f)));
            toolbeltHeight = config.Bind(section, "toolbeltHeight", -0.55f, new ConfigDescription("", new AcceptableValueRange<float>(-0.8f, -0.2f)));
            showHelmet = config.Bind(section, "helmetVisibility", true, "");
            hudScale = config.Bind(section, "hudScale", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.2f, 1.8f)));
            hudOpacity = config.Bind(section, "hudOpacity", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            controllerOrientedMovement = config.Bind(section, "movementControllerOriented", false, "");
            enableGesturePrompts = config.Bind(section, "showGesturePrompts", true, "");
            enableHandLaser = config.Bind(section, "showHandLaser", true, "");
            enableFeetMarker = config.Bind(section, "showFeetMarker", true, "");
            preventCursorLock = config.Bind(section, "disableCursorLock", true, "");
            debugMode = config.Bind(section, "debug", false, "");
            autoHideToolbelt = config.Bind(section, "autoHideToolbelt", false, "");
            bypassFatalErrors = config.Bind(section, "bypassFatalErrors", false, "");

            OnConfigChange?.Invoke();
        }
    }
}
#endif