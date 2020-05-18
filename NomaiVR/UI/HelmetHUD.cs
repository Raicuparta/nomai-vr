using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
{
    public class HelmetHUD : NomaiVRModule<HelmetHUD.Behaviour, HelmetHUD.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Transform _thrusterHUD;
            private static Transform _helmet;

            void Awake()
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
                _helmet.gameObject.AddComponent<SmoothFoolowParentRotation>();

                Camera.main.nearClipPlane = 0.01f;

                // Replace helmet model to prevent looking outside the edge.
                var helmetModelParent = _helmet.Find("HelmetRoot/HelmetMesh/HUD_Helmet_v2");
                var helmetModel = Instantiate(AssetLoader.HelmetPrefab, helmetModelParent);
                LayerHelper.ChangeLayerRecursive(helmetModel, "VisibleToPlayer");
                Destroy(helmetModelParent.Find("Helmet").gameObject);
                Destroy(helmetModelParent.Find("HelmetFrame").gameObject);
                helmetModel.AddComponent<ConditionalRenderer>().getShouldRender += () => Locator.GetPlayerSuit().IsWearingHelmet();

                // Adjust projected HUD.
                var surface = GameObject.Find("HUD_CurvedSurface").transform;
                surface.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += () => Locator.GetPlayerSuit().IsWearingHelmet();
                surface.transform.localScale = Vector3.one * 3.28f;
                surface.transform.localPosition = new Vector3(-0.06f, -0.44f, 0.1f);
                var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
                notifications.anchoredPosition = new Vector2(-200, -100);

                // Use default UI material so it doesn't look funky in stereo.
                var surfaceMaterial = surface.gameObject.GetComponent<MeshRenderer>().material;
                surfaceMaterial.shader = Canvas.GetDefaultCanvasMaterial().shader;
                surfaceMaterial.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
                surfaceMaterial.SetColor("_Color", new Color(1.5f, 1.5f, 1.5f, 1));

                var playerHUD = GameObject.Find("PlayerHUD");

                // Fix lock on UI on suit mode.
                var lockOnCanvas = playerHUD.transform.Find("HelmetOffUI/HelmetOffLockOn").GetComponent<Canvas>();
                lockOnCanvas.planeDistance = 10;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ThrustAndAttitudeIndicator>("LateUpdate", typeof(Patch), nameof(PatchLateUpdate));
                    NomaiVR.Post<HUDCamera>("Awake", typeof(Patch), nameof(PostHUDCameraAwake));
                }

                static void PostHUDCameraAwake(Camera ____camera)
                {
                    // Prevent distortion of helmet HUD.
                    ____camera.fieldOfView = 60;
                }

                static void PatchLateUpdate()
                {
                    // Fix thruster HUD rotation.
                    var rotation = _helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                    _thrusterHUD.transform.rotation = rotation;
                }
            }
        }
    }
}
