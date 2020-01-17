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

            var pose = hand.AddComponent<SteamVR_Behaviour_Pose>();
            pose.poseAction = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
            pose.inputSource = SteamVR_Input_Sources.RightHand;

            var interaction = hand.AddComponent<Valve.VR.InteractionSystem.Hand>();
            interaction.handType = SteamVR_Input_Sources.RightHand;
            interaction.grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
            interaction.grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
            interaction.hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");
            interaction.uiInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

            var grabGrip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
            grabGrip.onChange += GrabGrip_onChange;
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            //Invoke("InitSteamVR", 2);
        }

        private void GrabGrip_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            NomaiVR.Log("YO MR WHITwE");
        }
    }
}
