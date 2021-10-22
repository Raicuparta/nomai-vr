using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR.Hands
{
    internal class HandsController : NomaiVRModule<HandsController.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            public static Hand DominantHandBehaviour => !ModSettings.LeftHandDominant ? RightHandBehaviour : LeftHandBehaviour;
            public static Transform DominantHand => !ModSettings.LeftHandDominant ? RightHand : LeftHand;
            public static Transform OffHand => ModSettings.LeftHandDominant ? RightHand : LeftHand;
            public static Hand OffHandBehaviour => ModSettings.LeftHandDominant ? RightHandBehaviour : LeftHandBehaviour;
            public static Transform RightHand;
            public static Hand RightHandBehaviour;
            public static Transform LeftHand;
            public static Hand LeftHandBehaviour;
            private Transform wrapper;

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
                wrapper = activeCamera.transform.parent;
                var cameraObject = new GameObject();
                cameraObject.SetActive(false);
                cameraObject.tag = "MainCamera";
                var camera = cameraObject.AddComponent<Camera>();
                camera.transform.parent = wrapper;
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
                wrapper = new GameObject().transform;
                wrapper.parent = Camera.main.transform.parent;
                wrapper.localRotation = Quaternion.identity;
                wrapper.localPosition = Camera.main.transform.localPosition;
            }

            private void SetUpHands()
            {
                var right = new GameObject().AddComponent<Hand>();
                right.pose = SteamVR_Actions.default_RightHand;
                right.transform.parent = wrapper;
                right.transform.localPosition = Vector3.zero;
                right.transform.localRotation = Quaternion.identity;
                right.handPrefab = AssetLoader.HandPrefab;
                right.fallbackFist = AssetLoader.FallbackFistPose;
                right.fallbackPoint = AssetLoader.FallbackPointPose;
                right.fallbackRelax = AssetLoader.FallbackRelaxedPose;
                RightHand = right.transform;
                RightHandBehaviour = right;

                var left = new GameObject().AddComponent<Hand>();
                left.pose = SteamVR_Actions.default_LeftHand;
                left.transform.parent = wrapper;
                left.transform.localPosition = Vector3.zero;
                left.transform.localRotation = Quaternion.identity;
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
                if (SceneHelper.IsInGame() && wrapper && Camera.main)
                {
                    wrapper.localPosition = Camera.main.transform.localPosition - InputTracking.GetLocalPosition(XRNode.CenterEye);
                }
            }
        }
    }
}
