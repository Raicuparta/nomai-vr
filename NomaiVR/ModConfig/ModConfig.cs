using OWML.Common;
using System;
using UnityEngine;

namespace NomaiVR
{
    public class ModConfig
    {
        public static event Action<ModConfig> OnConfigChange;

        public readonly bool debugMode = true;
        public readonly bool preventCursorLock = true;
        public readonly bool showHelmet = true;
        public readonly int overrideRefreshRate = 0;
        public readonly float vibrationStrength = 1;
        public readonly bool enableGesturePrompts = true;
        public readonly bool controllerOrientedMovement = false;
        public readonly bool autoHideToolbelt = false;
        public readonly bool bypassFatalErrors = false;
        public readonly float toolbeltHeight = 1f;
        public readonly float hudScale = 1f;

        public ModConfig(IModConfig config)
        {
            overrideRefreshRate = config.GetSettingsValue<int>("refreshRateOverride");
            vibrationStrength = config.GetSettingsValue<float>("vibrationIntensity");
            showHelmet = config.GetSettingsValue<bool>("helmetVisibility");
            controllerOrientedMovement = config.GetSettingsValue<bool>("movementControllerOriented");
            enableGesturePrompts = config.GetSettingsValue<bool>("showGesturePrompts");
            preventCursorLock = config.GetSettingsValue<bool>("disableCursorLock");
            debugMode = config.GetSettingsValue<bool>("debug");
            autoHideToolbelt = config.GetSettingsValue<bool>("autoHideToolbelt");
            hudScale = config.GetSettingsValue<float>("hudScale");
            bypassFatalErrors = config.GetSettingsValue<bool>("bypassFatalErrors");

            // OWML doesn't support negative slider values so I subtract it here.
            toolbeltHeight = config.GetSettingsValue<float>("toolbeltHeight") - 1f;

            if (preventCursorLock)
            {
                NomaiVRPatch.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            OnConfigChange?.Invoke(this);
        }
    }
}
