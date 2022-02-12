using System;

namespace NomaiVR.ModConfig
{
    public interface IModSettingProvider
    {
        event Action OnConfigChange;

        bool LeftHandDominant { get; }
        bool DebugMode { get; }
        bool PreventCursorLock { get; }
        bool ShowHelmet { get; }
        float VibrationStrength { get; }
        bool EnableGesturePrompts { get; }
        bool EnableHandLaser { get; }
        bool EnableFeetMarker { get; }
        bool EnableLookArrow { get; }
        bool ControllerOrientedMovement { get; }
        bool AutoHideToolbelt { get; }
        float ToolbeltHeight { get; }
        float HudScale { get; }
        float HudOpacity { get; }
        bool HudSmoothFollow { get; }

        void Configure();
    }
}
