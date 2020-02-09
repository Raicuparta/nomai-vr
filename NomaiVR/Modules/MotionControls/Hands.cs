using OWML.ModHelper.Events;
using System;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR {
    public class Hands: MonoBehaviour {
        public static Transform RightHand;
        public static Transform LeftHand;
        GameObject _handPrefab;
        GameObject _glovePrefab;
        Transform _wrapper;

        private void Start () {
            var assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/hands");
            NomaiVR.Log("assets:", String.Join(" ", assetBundle.GetAllAssetNames()));
            _handPrefab = assetBundle.LoadAsset<GameObject>("assets/righthandprefab.prefab");
            _glovePrefab = assetBundle.LoadAsset<GameObject>("assets/rightgloveprefab.prefab");

            _wrapper = new GameObject().transform;
            RightHand = CreateHand(SteamVR_Actions.default_RightHand, Quaternion.Euler(314f, 12.7f, 281f), new Vector3(0.02f, 0.06f, -0.2f), _wrapper);
            LeftHand = CreateHand(SteamVR_Actions.default_LeftHand, Quaternion.Euler(317.032f, 347.616f, 76.826f), new Vector3(-0.05f, 0.07f, -0.2f), _wrapper, true);

            _wrapper.parent = Camera.main.transform.parent;
            _wrapper.localRotation = Quaternion.identity;
            _wrapper.localPosition = Camera.main.transform.localPosition;

            HideBody();
            gameObject.AddComponent<FlashlightGesture>();
            gameObject.AddComponent<HoldHUD>();
            gameObject.AddComponent<HoldMallowStick>();
            gameObject.AddComponent<HoldProbeLauncher>();
            gameObject.AddComponent<HoldTranslator>();
            gameObject.AddComponent<HoldSignalscope>();
            gameObject.AddComponent<HoldItem>();
            gameObject.AddComponent<HoldPrompts>();
            gameObject.AddComponent<LaserPointer>();
        }

        bool ShouldRenderGloves () {
            return Locator.GetPlayerSuit().IsWearingSuit(true);
        }

        bool ShouldRenderHands () {
            return !Locator.GetPlayerSuit().IsWearingSuit(true);
        }

        Transform CreateHand (SteamVR_Action_Pose pose, Quaternion rotation, Vector3 position, Transform wrapper, bool isLeft = false) {
            var hand = Instantiate(_handPrefab).transform;
            var glove = Instantiate(_glovePrefab).transform;
            hand.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderHands;
            glove.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderGloves;
            var handParent = new GameObject().transform;
            handParent.parent = wrapper;
            hand.parent = handParent;
            hand.localPosition = position;
            hand.localRotation = rotation;
            hand.localScale = Vector3.one * 6;
            glove.parent = handParent;
            glove.localPosition = position;
            glove.localRotation = rotation;
            glove.localScale = Vector3.one * 6;
            if (isLeft) {
                hand.localScale = new Vector3(-hand.localScale.x, hand.localScale.y, hand.localScale.z);
                glove.localScale = new Vector3(-glove.localScale.x, glove.localScale.y, glove.localScale.z);
            }

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
