using OWML.Common;
using System;
using UnityEngine;

namespace NomaiVR
{
    public static class ModSettings
    {
        public static event Action OnConfigChange;

        public static bool DebugMode { get; private set; } = true;
        public static bool PreventCursorLock { get; private set; }
        public static bool ShowHelmet { get; private set; }
        public static int OverrideRefreshRate { get; private set; }
        public static float VibrationStrength { get; private set; }
        public static bool EnableGesturePrompts { get; private set; }
        public static bool ControllerOrientedMovement { get; private set; }
        public static bool AutoHideToolbelt { get; private set; }
        public static bool BypassFatalErrors { get; private set; }
        public static float ToolbeltHeight { get; private set; }
        public static float HudScale { get; private set; }

        public static void SetConfig(IModConfig config)
        {
            OverrideRefreshRate = config.GetSettingsValue<int>("refreshRateOverride");
            VibrationStrength = config.GetSettingsValue<float>("vibrationIntensity");
            ShowHelmet = config.GetSettingsValue<bool>("helmetVisibility");
            ControllerOrientedMovement = config.GetSettingsValue<bool>("movementControllerOriented");
            EnableGesturePrompts = config.GetSettingsValue<bool>("showGesturePrompts");
            PreventCursorLock = config.GetSettingsValue<bool>("disableCursorLock");
            DebugMode = config.GetSettingsValue<bool>("debug");
            AutoHideToolbelt = config.GetSettingsValue<bool>("autoHideToolbelt");
            HudScale = config.GetSettingsValue<float>("hudScale");
            BypassFatalErrors = config.GetSettingsValue<bool>("bypassFatalErrors");

            // OWML doesn't support negative slider values so I subtract it here.
            ToolbeltHeight = config.GetSettingsValue<float>("toolbeltHeight") - 1f;

            if (PreventCursorLock)
            {
                NomaiVRPatch.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            OnConfigChange?.Invoke();
        }
    }
}
