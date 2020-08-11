using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class FatalErrorChecker
    {
        const string SupportedVersion = "1.0.7";

        public FatalErrorChecker()
        {
            CheckGameVersion();
            CheckControllerState();
        }

        private void CheckControllerState()
        {
            SteamVR.instance.hmd.IsTrackedDeviceConnected(0);
            SteamVR.instance.hmd.IsTrackedDeviceConnected(1);

            if (!SteamVR.instance.hmd.IsTrackedDeviceConnected(0))
            {
                Logs.WriteFatal("Trackerd 0 failed");
            }
            if (!SteamVR.instance.hmd.IsTrackedDeviceConnected(1))
            {
                Logs.WriteFatal("Trackerd 1 failed");
            }
        }

        private void CheckGameVersion()
        {
            if (!IsGameVersionSupported())
            {
                Logs.WriteFatal(
                    $"Fatal error: this version of NomaiVR only supports Outer Wilds {SupportedVersion}.\n" +
                    $"Currently installed version of Outer Wilds is {Application.version}.\n" +
                    "Make sure you are using the latest version of NomaiVR"
                );
            }
        }

        private bool IsGameVersionSupported()
        {
            string[] gameVersionParts = SplitVersion(Application.version);
            string[] supportedVersionParts = SplitVersion(SupportedVersion);

            for (int i = 0; i < supportedVersionParts.Length; i++)
            {
                if (gameVersionParts[i] != supportedVersionParts[i])
                {
                    return false;
                }
            }

            return true;
        }

        private string[] SplitVersion(string version)
        {
            return version.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
