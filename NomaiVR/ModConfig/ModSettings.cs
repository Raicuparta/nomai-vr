using System;
using BepInEx.Configuration;
using UnityEngine;

namespace NomaiVR
{
    public static class ModSettings
    {
        public static event Action OnConfigChange;

        private static ConfigEntry<bool> leftHandDominant;
        public static bool LeftHandDominant => leftHandDominant.Value;

        private static ConfigEntry<bool> debugMode;
        public static bool DebugMode => debugMode.Value;

        private static ConfigEntry<bool> preventCursorLock;
        public static bool PreventCursorLock => preventCursorLock.Value;

        private static ConfigEntry<bool> showHelmet;
        public static bool ShowHelmet => showHelmet.Value;

        private static ConfigEntry<int> overrideRefreshRate;
        public static int OverrideRefreshRate => overrideRefreshRate.Value;

        private static ConfigEntry<float> vibrationStrength;
        public static float VibrationStrength => vibrationStrength.Value;

        private static ConfigEntry<bool> enableGesturePrompts;
        public static bool EnableGesturePrompts => enableGesturePrompts.Value;

        private static ConfigEntry<bool> enableHandLaser;
        public static bool EnableHandLaser => enableHandLaser.Value;

        private static ConfigEntry<bool> enableFeetMarker;
        public static bool EnableFeetMarker => enableFeetMarker.Value;

        private static ConfigEntry<bool> controllerOrientedMovement;
        public static bool ControllerOrientedMovement => controllerOrientedMovement.Value;

        private static ConfigEntry<bool> autoHideToolbelt;
        public static bool AutoHideToolbelt => autoHideToolbelt.Value;

        private static ConfigEntry<bool> bypassFatalErrors;
        public static bool BypassFatalErrors => bypassFatalErrors.Value;

        private static ConfigEntry<float> toolbeltHeight;
        public static float ToolbeltHeight => toolbeltHeight.Value;

        private static ConfigEntry<float> hudScale;
        public static float HudScale => hudScale.Value;

        private static ConfigEntry<float> hudOpacity;
        public static float HudOpacity => hudOpacity.Value;

        public static void SetConfig(ConfigFile config)
        {
            // TODO: setting descriptions.
            const string section = "NomaiVRSettings";
            leftHandDominant = config.Bind(section, "leftHandDominant", false, "");
            overrideRefreshRate = config.Bind(section, "refreshRateOverride", 0, new ConfigDescription("", new AcceptableValueList<int>(0, 30, 60, 70, 72, 80, 90, 120, 144)));
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

            if (PreventCursorLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            OnConfigChange?.Invoke();
        }
    }
}
