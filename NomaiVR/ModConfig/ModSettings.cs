using System;
using UnityEngine;

namespace NomaiVR
{
    public static class ModSettings
    {
        public static event Action OnConfigChange;

        public static bool LeftHandDominant { get; private set; } = false;
        public static bool DebugMode { get; private set; } = true;
        public static bool PreventCursorLock { get; private set; }
        public static bool ShowHelmet { get; private set; }
        public static int OverrideRefreshRate { get; private set; }
        public static float VibrationStrength { get; private set; }
        public static bool EnableGesturePrompts { get; private set; }
        public static bool EnableHandLaser { get; private set; }
        public static bool EnableFeetMarker { get; private set; }
        public static bool ControllerOrientedMovement { get; private set; }
        public static bool AutoHideToolbelt { get; private set; }
        public static bool BypassFatalErrors { get; private set; }
        public static float ToolbeltHeight { get; private set; }
        public static float HudScale { get; private set; }
        public static float HudOpacity { get; private set; }

        // TODO ModConfig
        public static void SetConfig(/*IModConfig config*/)
        {
            //LeftHandDominant = config.GetSettingsValue<bool>("leftHandDominant");
            //OverrideRefreshRate = config.GetSettingsValue<int>("refreshRateOverride");
            //VibrationStrength = config.GetSettingsValue<float>("vibrationIntensity");
            //ShowHelmet = config.GetSettingsValue<bool>("helmetVisibility");
            //ControllerOrientedMovement = config.GetSettingsValue<bool>("movementControllerOriented");
            //EnableGesturePrompts = config.GetSettingsValue<bool>("showGesturePrompts");
            //EnableHandLaser = config.GetSettingsValue<bool>("showHandLaser");
            //EnableFeetMarker = config.GetSettingsValue<bool>("showFeetMarker");
            //PreventCursorLock = config.GetSettingsValue<bool>("disableCursorLock");
            //DebugMode = config.GetSettingsValue<bool>("debug");
            //AutoHideToolbelt = config.GetSettingsValue<bool>("autoHideToolbelt");
            //HudScale = config.GetSettingsValue<float>("hudScale");
            //HudOpacity = config.GetSettingsValue<float>("hudOpacity");
            //BypassFatalErrors = config.GetSettingsValue<bool>("bypassFatalErrors");

            //// OWML doesn't support negative slider values so I subtract it here.
            //ToolbeltHeight = config.GetSettingsValue<float>("toolbeltHeight") - 1f;

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
