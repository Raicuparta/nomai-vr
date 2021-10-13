using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NomaiVR.ModConfig
{
    public interface IModSettingProvider
    {
        event Action OnConfigChange;

        bool LeftHandDominant { get; }
        bool DebugMode { get; }
        bool PreventCursorLock { get; }
        bool ShowHelmet { get; }
        int OverrideRefreshRate { get; }
        float VibrationStrength { get; }
        bool EnableGesturePrompts { get; }
        bool EnableHandLaser { get; }
        bool EnableFeetMarker { get; }
        bool ControllerOrientedMovement { get; }
        bool AutoHideToolbelt { get; }
        bool BypassFatalErrors { get; }
        float ToolbeltHeight { get; }
        float HudScale { get; }
        float HudOpacity { get; }

        void Configure();
    }
}
