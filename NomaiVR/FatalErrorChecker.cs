using System;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    internal class FatalErrorChecker : NomaiVRModule<FatalErrorChecker.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private const string supportedVersion = "1.1.10";

        public static void ThrowSteamVRError()
        {
            Logs.WriteFatal(
                "Failed to initialize SteamVR. This could be for a few different reasons. Try these before starting the game again:" +
                "\n\n- For some people, it only works if SteamVR is already running before starting the game. For others, only when SteamVR is closed (SteamVR will open automatically). Try both and stick with what works for you;" +
                "\n\n- Make sure your headset and both of your VR controllers are connected and working;" +
                "\n\n- If you have the game on Steam:" +
                "\n--- Right-click Outer Wilds on your Steam library" +
                "\n--- Select \"Properties...\"" +
                "\n--- Disable \"Use Desktop Game Theatre.\"" +
                "\n\n If all else fails, try a version of the game from a different store (Steam / Epic)." +
                "\n Most people have better luck with Epic, but some people can only get it to work with the Steam version."
            );
        }

        public class Behaviour : MonoBehaviour
        {
            private bool isSteamVRInitialized;

            internal void Start()
            {
                CheckGameVersion();
            }

            internal void Update()
            {
                if (isSteamVRInitialized || SteamVR.initializing)
                {
                    return;
                }
                if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeFailure)
                {
                    ThrowSteamVRError();
                    isSteamVRInitialized = true;
                }
                else if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess)
                {
                    isSteamVRInitialized = true;
                }
            }

            private void CheckGameVersion()
            {
                if (!IsGameVersionSupported())
                {
                    Logs.WriteFatal(
                        $"Fatal error: this version of NomaiVR only supports Outer Wilds {supportedVersion}.\n" +
                        $"Currently installed version of Outer Wilds is {Application.version}.\n" +
                        "Make sure you are using the latest version of NomaiVR"
                    );
                }
            }

            private bool IsGameVersionSupported()
            {
                var gameVersionParts = SplitVersion(Application.version);
                var supportedVersionParts = SplitVersion(supportedVersion);

                for (var i = 0; i < supportedVersionParts.Length; i++)
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
}
