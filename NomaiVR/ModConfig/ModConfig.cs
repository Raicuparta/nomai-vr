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

            if (preventCursorLock)
            {
                NomaiVRPatch.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
