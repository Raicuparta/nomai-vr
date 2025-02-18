using System;
using UnityEngine;

namespace NomaiVR.ModConfig
{
    public static class ModSettings
    {
        public static event Action OnConfigChange;
        private static IModSettingProvider settingsProvider;

        public static bool LeftHandDominant => settingsProvider.LeftHandDominant;
        public static bool DebugMode => settingsProvider.DebugMode;
        public static bool PreventCursorLock => settingsProvider.PreventCursorLock;
        public static bool ShowHelmet => settingsProvider.ShowHelmet;
        public static float VibrationStrength => settingsProvider.VibrationStrength;
        public static bool EnableGesturePrompts => settingsProvider.EnableGesturePrompts;
        public static bool EnableHandLaser => settingsProvider.EnableHandLaser;
        public static bool EnableFeetMarker => settingsProvider.EnableFeetMarker;
        public static bool PreventClipping => settingsProvider.PreventClipping;
        public static bool FlashlightGesture => settingsProvider.FlashlightGesture;
        public static bool ControllerOrientedMovement => settingsProvider.ControllerOrientedMovement;
        public static bool SnapTurning => settingsProvider.SnapTurning;
        public static float SnapTurnIncrement => settingsProvider.SnapTurnIncrement;
        public static bool AutoHideToolbelt => settingsProvider.AutoHideToolbelt;
        public static float ToolbeltHeight => settingsProvider.ToolbeltHeight;
        public static float HudScale => settingsProvider.HudScale;
        public static float HudOpacity => settingsProvider.HudOpacity;
        public static float MarkersOpacity => settingsProvider.MarkersOpacity;
        public static float LookArrowOpacity => settingsProvider.LookArrowOpacity;
        public static bool HudSmoothFollow => settingsProvider.HudSmoothFollow;

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
