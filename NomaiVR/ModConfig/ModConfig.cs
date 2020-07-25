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
            overrideRefreshRate = config.GetSettingsValue<int>("overrideRefreshRate");
            vibrationStrength = config.GetSettingsValue<float>("vibrationStrength");
            showHelmet = config.GetSettingsValue<bool>("showHelmet");
            hideHudInConversations = config.GetSettingsValue<bool>("hideHudInConversations");
            controllerOrientedMovement = config.GetSettingsValue<bool>("controllerOrientedMovement");
            enableGesturePrompts = config.GetSettingsValue<bool>("enableGesturePrompts");
            preventCursorLock = config.GetSettingsValue<bool>("preventCursorLock");
            debugMode = config.GetSettingsValue<bool>("debugMode");

            if (preventCursorLock)
            {
                NomaiVRPatch.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
