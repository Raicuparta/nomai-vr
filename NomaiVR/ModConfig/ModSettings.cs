using System;
using NomaiVR.ModConfig;
using UnityEngine;

namespace NomaiVR
{
    public static class ModSettings
    {
        public static event Action OnConfigChange;
        private static IModSettingProvider settingsProvider;

        public static bool LeftHandDominant => settingsProvider.LeftHandDominant;
        public static bool DebugMode => settingsProvider.DebugMode;
        public static bool PreventCursorLock => settingsProvider.PreventCursorLock;
        public static bool ShowHelmet => settingsProvider.ShowHelmet;
        public static int OverrideRefreshRate => settingsProvider.OverrideRefreshRate;
        public static float VibrationStrength => settingsProvider.VibrationStrength;
        public static bool EnableGesturePrompts => settingsProvider.EnableGesturePrompts;
        public static bool EnableHandLaser => settingsProvider.EnableHandLaser;
        public static bool EnableFeetMarker => settingsProvider.EnableFeetMarker;
        public static bool ControllerOrientedMovement => settingsProvider.ControllerOrientedMovement;
        public static bool AutoHideToolbelt => settingsProvider.AutoHideToolbelt;
        public static bool BypassFatalErrors => settingsProvider.BypassFatalErrors;
        public static float ToolbeltHeight => settingsProvider.ToolbeltHeight;
        public static float HudScale => settingsProvider.HudScale;
        public static float HudOpacity => settingsProvider.HudOpacity;

        public static void SetProvider(IModSettingProvider provider)
        {
            if (settingsProvider != null) settingsProvider.OnConfigChange -= OnConfigChanged;

            settingsProvider = provider;

            provider.OnConfigChange += OnConfigChanged;
            provider.Configure();

            if (PreventCursorLock)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private static void OnConfigChanged()
        {
            OnConfigChange?.Invoke();
        }
    }
}
