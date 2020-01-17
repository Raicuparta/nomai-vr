using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace NomaiVR
{
    class SteamVRTest : MonoBehaviour
    {
        void Start() {
            NomaiVR.Log("Started SteamVRTest");

            var hand = new GameObject();

            SteamVR.Initialize();

            var grabGrip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("X");
            grabGrip.onChange += GrabGrip_onChange;
        }

        private void GrabGrip_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            NomaiVR.Log("YO MR X " + fromAction.localizedOriginName);
        }
    }
}
