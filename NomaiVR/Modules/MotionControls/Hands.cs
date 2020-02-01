using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR {
    public class Hands: MonoBehaviour {
        public static Transform RightHand;
        public static Transform LeftHand;
        Transform _wrapper;

        private void Start () {
            _wrapper = new GameObject().transform;
            RightHand = CreateHand("PlayerSuit_Glove_Right", SteamVR_Actions.default_RightPose, Quaternion.Euler(45, 180, 0), _wrapper);
            LeftHand = CreateHand("PlayerSuit_Glove_Left", SteamVR_Actions.default_LeftPose, Quaternion.Euler(-40, 330, 20), _wrapper);
            _wrapper.parent = Common.MainCamera.transform.parent;
            _wrapper.localRotation = Quaternion.identity;
            _wrapper.localPosition = Common.MainCamera.transform.localPosition;

            HideArms();
            gameObject.AddComponent<FlashlightGesture>();
            gameObject.AddComponent<HoldHUD>();
            gameObject.AddComponent<HoldMallowStick>();
            gameObject.AddComponent<HoldProbeLauncher>();
            gameObject.AddComponent<HoldTranslator>();
            gameObject.AddComponent<HoldSignalscope>();
            gameObject.AddComponent<HoldItem>();
            gameObject.AddComponent<LaserPointer>();
        }

        Transform CreateHand (string objectName, SteamVR_Action_Pose pose, Quaternion rotation, Transform wrapper) {
            var hand = Instantiate(GameObject.Find("SpaceSuit").transform.Find("Props_HEA_PlayerSuit_Hanging/" + objectName).gameObject).transform;
            var handParent = new GameObject().transform;
            handParent.parent = wrapper;
            hand.parent = handParent;
            hand.localPosition = new Vector3(0, -0.03f, -0.08f);
            hand.localRotation = rotation;
            hand.localScale = Vector3.one * 0.5f;

            handParent.gameObject.SetActive(false);
            var poseDriver = handParent.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;
            handParent.gameObject.SetActive(true);

            return handParent;
        }

        void HideArms () {
            var palyerMeshes = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");

            var suitMesh = palyerMeshes.Find("Traveller_Mesh_v01:Traveller_Geo");
            suitMesh.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject.SetActive(false);
            suitMesh.Find("Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject.SetActive(false);
            suitMesh.Find("Traveller_Mesh_v01:Props_HEA_Jetpack").gameObject.SetActive(false);

            var bodyMesh = palyerMeshes.Find("player_mesh_noSuit:Traveller_HEA_Player");
            bodyMesh.Find("player_mesh_noSuit:Player_RightArm").gameObject.SetActive(false);
            bodyMesh.Find("player_mesh_noSuit:Player_LeftArm").gameObject.SetActive(false);
        }

        public static Transform HoldObject (Transform objectTransform, Transform hand, Vector3 position, Quaternion rotation) {
            var objectParent = new GameObject().transform;
            objectParent.parent = hand;
            objectParent.localPosition = position;
            objectParent.localRotation = rotation;
            objectTransform.transform.parent = objectParent;
            objectTransform.transform.localPosition = Vector3.zero;
            objectTransform.transform.localRotation = Quaternion.identity;

            var tool = objectTransform.gameObject.GetComponent<PlayerTool>();
            if (tool) {
                tool.SetValue("_stowTransform", null);
                tool.SetValue("_holdTransform", null);
            }

            return objectParent;
        }

        public static Transform HoldObject (Transform objectTransform, Transform hand) {
            return HoldObject(objectTransform, hand, Vector3.zero, Quaternion.identity);
        }
        public static Transform HoldObject (Transform objectTransform, Transform hand, Quaternion rotation) {
            return HoldObject(objectTransform, hand, Vector3.zero, rotation);
        }
        public static Transform HoldObject (Transform objectTransform, Transform hand, Vector3 position) {
            return HoldObject(objectTransform, hand, position, Quaternion.identity);
        }

        void Update () {
            if (_wrapper) {
                _wrapper.localPosition = Common.MainCamera.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
            }
        }
    }
}
