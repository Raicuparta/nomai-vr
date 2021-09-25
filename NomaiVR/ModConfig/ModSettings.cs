using BepInEx.Configuration;
using System;
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


        // TODO ModConfig
        public static void SetConfig(ConfigFile config)
        {
            const string section = "NomaiVRSettings";
            leftHandDominant = config.Bind(section, "leftHandDominant", false, "");
            overrideRefreshRate = config.Bind(section, "refreshRateOverride", 0, "");
            vibrationStrength = config.Bind(section, "vibrationIntensity", 1f, "");
            showHelmet = config.Bind(section, "helmetVisibility", true, "");
            controllerOrientedMovement = config.Bind(section, "movementControllerOriented", false, "");
            enableGesturePrompts = config.Bind(section, "showGesturePrompts", true, "");
            enableHandLaser = config.Bind(section, "showHandLaser", true, "");
            enableFeetMarker = config.Bind(section, "showFeetMarker", true, "");
            preventCursorLock = config.Bind(section, "disableCursorLock", false, "");
            debugMode = config.Bind(section, "debug", false, "");
            autoHideToolbelt = config.Bind(section, "autoHideToolbelt", false, "");
            hudScale = config.Bind(section, "hudScale", 1f, "");
            hudOpacity = config.Bind(section, "hudOpacity", 1f, "");
            bypassFatalErrors = config.Bind(section, "bypassFatalErrors", false, "");

            // TODO comment
            // OWML doesn't support negative slider values so I subtract it here.
            toolbeltHeight = config.Bind(section, "toolbeltHeight", -0.55f, ""); // min: 0.2 - 1; max: 0.8 - 1.

            if (PreventCursorLock)
            {
                // TODO: disable cursor
                // NomaiVRPatch.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            OnConfigChange?.Invoke();
        }
    }
}
