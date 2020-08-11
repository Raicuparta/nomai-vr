using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    public class FatalErrorChecker
    {
        const string SupportedVersion = "1.0.5";

        public FatalErrorChecker()
        {
            CheckGameVersion();
        }

        public static void CheckGameVersion()
        {
            if (!IsGameVersionSupported())
            {
                Logs.WriteFatal(
                    $"Fatal error: this version of NomaiVR only supports Outer Wilds {SupportedVersion}. " +
                    $"Make sure you are using the latest version of NomaiVR. " +
                    $"Currently installed version of Outer Wilds is {Application.version}. " +
                    $"You can force the game to start anyway by setting skipGameVersionCheck to true in NomaiVR/config.json."
                );
            }
        }

        private static bool IsGameVersionSupported()
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

        private static string[] SplitVersion(string version)
        {
            return version.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
