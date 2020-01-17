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

        void InitSteamVR() {
            var hand = new GameObject();
            hand.SetActive(false);

            var pose = hand.AddComponent<SteamVR_Behaviour_Pose>();
            pose.poseAction = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");
            pose.inputSource = SteamVR_Input_Sources.RightHand;

            var interaction = hand.AddComponent<Valve.VR.InteractionSystem.Hand>();
            interaction.handType = SteamVR_Input_Sources.RightHand;
            interaction.grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");
            interaction.grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
            interaction.hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");
            interaction.uiInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");
            interaction.useHoverSphere = false;
            interaction.useFingerJointHover = false;
            interaction.useControllerHoverComponent = false;
            interaction.useGUILayout = false;
           // interaction.renderModelPrefab = GameObject.Find("SpaceSuit");
            //interaction.renderModelPrefab = GameObject.Find("SpaceSuit").transform.Find("Props_HEA_PlayerSuit_Hanging/PlayerSuit_Glove_Right").gameObject;
            interaction.renderModelPrefab = new GameObject();
            //NomaiVR.Log("interaction.renderModelPrefab " + interaction.renderModelPrefab.name);

            //var player = Common.PlayerBody.gameObject.AddComponent<Valve.VR.InteractionSystem.Player>().gameObject;

            var vrPlayer = new GameObject().AddComponent<Valve.VR.InteractionSystem.Player>();
            vrPlayer.rigSteamVR = new GameObject();
            vrPlayer.rig2DFallback = new GameObject();
            vrPlayer.hmdTransforms = new[] { Camera.main.transform };
            vrPlayer.hands = new[] { interaction };
            vrPlayer.headsetOnHead = SteamVR_Input.GetBooleanAction("HeadsetOnHead");
            vrPlayer.trackingOriginTransform = new GameObject().transform;
            //vrPlayer.trackingOriginTransform = new GameObject().transform;

            var grabGrip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
            grabGrip.onChange += GrabGrip_onChange;

            //player.SetActive(true);
            hand.SetActive(true);
        }

        private void GrabGrip_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            NomaiVR.Log("YO MR WHITE");
        }
    }
}
