using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    internal class HandsController : NomaiVRModule<HandsController.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Transform RightHand;
            public static Hand RightHandBehaviour;
            public static Transform LeftHand;
            public static Hand LeftHandBehaviour;
            private Transform _wrapper;

            internal void Start()
            {
                if (SceneHelper.IsInTitle())
                {
                    SetUpWrapperTittle();
                }

                if (SceneHelper.IsInGame())
                {
                    SetUpWrapperInGame();
                    HideBody();
                }

                SetUpHands();
            }

            private void SetUpWrapperTittle()
            {
                var activeCamera = Locator.GetActiveCamera();
                activeCamera.gameObject.SetActive(false);
                _wrapper = activeCamera.transform.parent;
                var cameraObject = new GameObject();
                cameraObject.SetActive(false);
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                camera.transform.parent = _wrapper;
                camera.transform.localPosition = Vector3.zero;
                camera.transform.localRotation = Quaternion.identity;

                camera.nearClipPlane = activeCamera.nearClipPlane;
                camera.farClipPlane = activeCamera.farClipPlane;
                camera.clearFlags = activeCamera.clearFlags;
                camera.backgroundColor = activeCamera.backgroundColor;
                camera.cullingMask = activeCamera.cullingMask;
                camera.depth = activeCamera.mainCamera.depth;
                camera.tag = activeCamera.tag;

                var owCamera = cameraObject.AddComponent<OWCamera>();
                owCamera.renderSkybox = true;

                cameraObject.AddComponent<FlareLayer>();
                cameraObject.SetActive(true);

                cameraObject.AddComponent<Light>();
            }

            private void SetUpWrapperInGame()
            {
                _wrapper = new GameObject().transform;
                _wrapper.parent = Camera.main.transform.parent;
                _wrapper.localRotation = Quaternion.identity;
                _wrapper.localPosition = Camera.main.transform.localPosition;
            }

            private void SetUpHands()
            {
                var right = new GameObject().AddComponent<Hand>();
                right.pose = SteamVR_Actions.default_RightHand;
                right.transform.parent = _wrapper;
                right.transform.localPosition = Vector3.zero;//new Vector3(0.03f, 0.05f, -0.2f);
                right.transform.localRotation = Quaternion.identity; //Quaternion.Euler(313f, 10f, 295f);
                right.handPrefab = AssetLoader.HandPrefab;
                right.fallbackFist = AssetLoader.FallbackFistPose;
                right.fallbackPoint = AssetLoader.FallbackPointPose;
                right.fallbackRelax = AssetLoader.FallbackRelaxedPose;
                RightHand = right.transform;
                RightHandBehaviour = right;

                var left = new GameObject().AddComponent<Hand>();
                left.pose = SteamVR_Actions.default_LeftHand;
                left.transform.parent = _wrapper;
                left.transform.localPosition = Vector3.zero;//new Vector3(-0.03f, 0.05f, -0.2f);
                left.transform.localRotation = Quaternion.identity; //Quaternion.Euler(313f, 350f, 65f);
                left.isLeft = true;
                left.handPrefab = AssetLoader.HandPrefab;
                left.fallbackFist = AssetLoader.FallbackFistPose;
                left.fallbackPoint = AssetLoader.FallbackPointPose;
                left.fallbackRelax = AssetLoader.FallbackRelaxedPose;
                LeftHand = left.transform;
                LeftHandBehaviour = left;
            }

            private static void HideBody()
            {
                var bodyModels = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");

                // Legs, torso and head are kept visible to the probe camera,
                // so we can still take some selfies when we're feelinf cute.
                var renderers = bodyModels.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer.name.Contains("ShadowCaster") || renderer.name.Contains("Head") || renderer.name.Contains("Helmet"))
                    {
                        continue;
                    }

                    // Make the player body shadows visible to the player camera.
                    var shadowCaster = Instantiate(renderer);
                    shadowCaster.transform.parent = renderer.transform;
                    shadowCaster.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                    // Make this body mesh only visible to the probe launcher camera.
                    renderer.gameObject.layer = LayerMask.NameToLayer("VisibleToProbe");
                }

                // Arms are always hidden, since we have our own motion-controlled hands.
                var withoutSuit = bodyModels.Find("player_mesh_noSuit:Traveller_HEA_Player");
                withoutSuit.Find("player_mesh_noSuit:Player_LeftArm").gameObject.SetActive(false);
                withoutSuit.Find("player_mesh_noSuit:Player_RightArm").gameObject.SetActive(false);
                var withSuit = bodyModels.Find("Traveller_Mesh_v01:Traveller_Geo");
                withSuit.Find("Traveller_Mesh_v01:PlayerSuit_LeftArm").gameObject.SetActive(false);
                withSuit.Find("Traveller_Mesh_v01:PlayerSuit_RightArm").gameObject.SetActive(false);
            }

            internal void Update()
            {
                if (SceneHelper.IsInGame() && _wrapper && Camera.main)
                {
                    _wrapper.localPosition = Camera.main.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
                }
            }
        }
    }
}
