using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.XR;

namespace NomaiVR
{
    public class ModConfig
    {
        public bool showMirrorView = true;
        public bool debugMode = true;
        public bool preventCursorLock = true;
        public bool showHelmet = true;
        public int overrideRefreshRate = 0;

        public ModConfig()
        {
            XRSettings.showDeviceView = showMirrorView;

            if (preventCursorLock)
            {
                NomaiVR.Empty<CursorManager>("Update");
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
