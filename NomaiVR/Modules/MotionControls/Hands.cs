﻿using OWML.ModHelper.Events;
using System;
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
            RightHand = CreateHand("PlayerSuit_Glove_Right", SteamVR_Actions.default_RightHand, Quaternion.Euler(45, 180, 0), _wrapper);
            LeftHand = CreateHand("PlayerSuit_Glove_Left", SteamVR_Actions.default_LeftHand, Quaternion.Euler(-40, 330, 20), _wrapper);
            _wrapper.parent = Camera.main.transform.parent;
            _wrapper.localRotation = Quaternion.identity;
            _wrapper.localPosition = Camera.main.transform.localPosition;

            HideBody();
            //gameObject.AddComponent<FlashlightGesture>();
            //gameObject.AddComponent<HoldHUD>();
            //gameObject.AddComponent<HoldMallowStick>();
            //gameObject.AddComponent<HoldProbeLauncher>();
            //gameObject.AddComponent<HoldTranslator>();
            //gameObject.AddComponent<HoldSignalscope>();
            //gameObject.AddComponent<HoldItem>();
            //gameObject.AddComponent<HoldPrompts>();
            //gameObject.AddComponent<LaserPointer>();
        }

        Transform CreateHand (string objectName, SteamVR_Action_Pose pose, Quaternion rotation, Transform wrapper) {
            var hands = NomaiVR.Helper.Assets.LoadBundle("assets/hands");
            NomaiVR.Log(String.Join(" ", hands.GetAllAssetNames()));
            var hand = hands.LoadAsset<GameObject>("assets/RightHandPrefab.prefab").transform;
            NomaiVR.Log("loaded", hand.name);
            //var hand = Instantiate(GameObject.Find("SpaceSuit").transform.Find("Props_HEA_PlayerSuit_Hanging/" + objectName).gameObject).transform;
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

        void HideBody () {
            Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2").gameObject.SetActive(false);
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
                _wrapper.localPosition = Camera.main.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
            }
        }
    }
}
