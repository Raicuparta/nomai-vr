using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class HelmetHUD: MonoBehaviour {
        private static Transform _thrusterHUD;
        private static Transform _helmet;
        static AssetBundle _assetBundle;
        static GameObject _helmetPrefab;

        void Awake () {
            if (!_assetBundle) {
                _assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/helmet");
                _helmetPrefab = _assetBundle.LoadAsset<GameObject>("assets/helmet.prefab");
            }

            _thrusterHUD = GameObject.Find("HUD_Thrusters").transform;

            // Add a stronger line pointing forward in the thruster HUD
            var forwardIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            forwardIndicator.parent = _thrusterHUD.Find("ThrusterArrows/Positive_Z");
            forwardIndicator.localPosition = Vector3.forward * 0.75f;
            forwardIndicator.localRotation = Quaternion.identity;
            forwardIndicator.localScale = new Vector3(0.05f, 0.05f, 1.5f);
            forwardIndicator.gameObject.layer = LayerMask.NameToLayer("HeadsUpDisplay");
            forwardIndicator.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Color");
            forwardIndicator.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);

            var animator = FindObjectOfType<HUDHelmetAnimator>();
            animator.SetValue("_helmetOffsetSpring", new DampedSpring3D());

            // Move helmet forward to make it a bit more visible.
            _helmet = animator.transform;
            _helmet.localPosition = Vector3.forward * -0.07f;
            _helmet.localScale = Vector3.one * 0.5f;
            _helmet.gameObject.AddComponent<SmoothFoolowParentRotation>();

            GameObject.Find("HUD_CurvedSurface").GetComponent<MeshRenderer>().material.shader = Shader.Find("UI/Default");
            GameObject.Find("HelmetVisorUVRenderer").GetComponent<MeshRenderer>().material.shader = Shader.Find("UI/Default");
            Destroy(GameObject.Find("HelmetVisorMaskRenderer"));

            var uiCanvas = _helmet.Find("PlayerHUD/HelmetOnUI/UICanvas").GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.gameObject.AddComponent<DebugTransform>();
            uiCanvas.transform.localPosition = new Vector3(0.14f, 0.58f, 0.82f);

            Camera.main.nearClipPlane = 0.01f;

            // Replace helmet model to prevent looking outside the edge.
            var helmetModelParent = _helmet.Find("HelmetRoot/HelmetMesh/HUD_Helmet_v2");
            var helmetModel = Instantiate(_helmetPrefab, helmetModelParent);
            Common.ChangeLayerRecursive(helmetModel, "VisibleToPlayer");
            Destroy(helmetModelParent.Find("Helmet").gameObject);
            Destroy(helmetModelParent.Find("HelmetFrame").gameObject);

            // Adjust projected HUD.
            var surface = GameObject.Find("HUD_CurvedSurface").transform;
            surface.transform.localScale = Vector3.one * 3.28f;
            surface.transform.localPosition = new Vector3(-0.06f, -0.76f, 0.06f);
            var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
            notifications.anchoredPosition = new Vector2(-200, -100);

            var playerHUD = GameObject.Find("PlayerHUD");

            // Fix lock on UI on suit mode.
            var lockOnCanvas = playerHUD.transform.Find("HelmetOffUI/HelmetOffLockOn").GetComponent<Canvas>();
            lockOnCanvas.planeDistance = 10;
        }

        //void LateUpdate () {
        //    if (_helmet == null) {
        //        return;
        //    }

        //    _helmet.position = Hands.LeftHand.position;
        //    _helmet.rotation = Hands.LeftHand.rotation;
        //}

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ThrustAndAttitudeIndicator>("LateUpdate", typeof(Patches), nameof(PatchLateUpdate));
                NomaiVR.Post<HUDCamera>("Awake", typeof(Patches), nameof(PostHUDCameraAwake));
            }

            static void PostHUDCameraAwake (Camera ____camera) {
                // Prevent distortion of helmet HUD.
                ____camera.fieldOfView = 60;
            }

            static void PatchLateUpdate () {
                // Fix thruster HUD rotation.
                var rotation = _helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                _thrusterHUD.transform.rotation = rotation;
            }
        }
    }
}
