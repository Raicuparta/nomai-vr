using OWML.Common;
using UnityEngine;

namespace NomaiVR
{
    public class ModConfig
    {
        public bool debugMode = true;
        public bool preventCursorLock = true;
        public bool showHelmet = true;
        public bool hideHudInConversations = false;
        public int overrideRefreshRate = 0;
        public float vibrationStrength = 1;
        public bool enableGesturePrompts = true;
        public bool controllerOrientedMovement = false;
        public bool autoHideToolbelt = false;
        public float toolbeltHeight = 0f;

        public ModConfig(IModConfig config)
        {
            overrideRefreshRate = config.GetSettingsValue<int>("refreshRateOverride");
            vibrationStrength = config.GetSettingsValue<float>("vibrationIntensity");
            showHelmet = config.GetSettingsValue<bool>("helmetVisibility");
            hideHudInConversations = config.GetSettingsValue<bool>("hideHudDuringDialogue");
            controllerOrientedMovement = config.GetSettingsValue<bool>("movementControllerOriented");
            enableGesturePrompts = config.GetSettingsValue<bool>("showGesturePrompts");
            preventCursorLock = config.GetSettingsValue<bool>("disableCursorLock");
            debugMode = config.GetSettingsValue<bool>("debug");
            autoHideToolbelt = config.GetSettingsValue<bool>("autoHideToolbelt");

            // OWML doesn't support negative slider values so I subtract it here.
            toolbeltHeight = config.GetSettingsValue<float>("toolbeltHeight") - 1f;
            NomaiVR.Log("toolbeltHeight is", toolbeltHeight);

            if (preventCursorLock)
            {
                NomaiVRPatch.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
