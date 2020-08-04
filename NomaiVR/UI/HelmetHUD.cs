using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    internal class HelmetHUD : NomaiVRModule<HelmetHUD.Behaviour, HelmetHUD.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Transform _thrusterHUD;
            private static Transform _helmet;

            internal void Awake()
            {
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
                _helmet.gameObject.AddComponent<SmoothFollowCameraRotation>();

                Camera.main.nearClipPlane = 0.01f;

                // Replace helmet model to prevent looking outside the edge.
                var helmetModelParent = _helmet.Find("HelmetRoot/HelmetMesh/HUD_Helmet_v2");
                helmetModelParent.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () =>
                    NomaiVR.Config.showHelmet;

                var helmetModel = Instantiate(AssetLoader.HelmetPrefab, helmetModelParent);
                LayerHelper.ChangeLayerRecursive(helmetModel, "VisibleToPlayer");
                Destroy(helmetModelParent.Find("Helmet").gameObject);
                Destroy(helmetModelParent.Find("HelmetFrame").gameObject);
                helmetModel.AddComponent<ConditionalRenderer>().getShouldRender += () => Locator.GetPlayerSuit().IsWearingHelmet();

                // Adjust projected HUD.
                var surface = GameObject.Find("HUD_CurvedSurface").transform;

                // re-use variables because getShouldRender is called every frame via Update
                // variables are not strictly necessary, but otherwise the expression would be quite long and hard to read
                bool isWearingHelmet;
                bool hideBecauseOfConversation;
                surface.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += () =>
                {
                    isWearingHelmet = Locator.GetPlayerSuit().IsWearingHelmet();
                    hideBecauseOfConversation = PlayerState.InConversation() && NomaiVR.Config.hideHudInConversations;

                    return isWearingHelmet && !hideBecauseOfConversation;
                };
                surface.transform.localScale = Vector3.one * 3.28f;
                surface.transform.localPosition = new Vector3(-0.06f, -0.44f, 0.1f);
                var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
                notifications.anchoredPosition = new Vector2(-200, -100);

                // Default HUD shader looks funky in stereo, so we need to replace it with something more standard.
                var surfaceRenderer = surface.gameObject.GetComponent<MeshRenderer>();
                surfaceRenderer.material.SetColor("_Color", new Color(1.5f, 1.5f, 1.5f, 1));
                MaterialHelper.MakeMaterialDrawOnTop(surfaceRenderer.material);

                var playerHUD = GameObject.Find("PlayerHUD");

                // Fix lock on UI on suit mode.
                var lockOnCanvas = playerHUD.transform.Find("HelmetOffUI/HelmetOffLockOn").GetComponent<Canvas>();
                lockOnCanvas.planeDistance = 10;
            }

            internal void Update()
            {
                if (_helmet != null)
                {
                    _helmet.transform.localScale = new Vector3(NomaiVR.Config.hudScale, NomaiVR.Config.hudScale, 1f) * 0.5f;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ThrustAndAttitudeIndicator>("LateUpdate", nameof(PatchLateUpdate));
                    Postfix<HUDCamera>("Awake", nameof(PostHUDCameraAwake));
                }

                private static void PostHUDCameraAwake(Camera ____camera)
                {
                    // Prevent distortion of helmet HUD.
                    ____camera.fieldOfView = 60;
                }

                private static void PatchLateUpdate()
                {
                    // Fix thruster HUD rotation.
                    var rotation = _helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                    _thrusterHUD.transform.rotation = rotation;
                }
            }
        }
    }
}
