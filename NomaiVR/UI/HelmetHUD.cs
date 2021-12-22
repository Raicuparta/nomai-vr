
using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.UI
{
    internal class HelmetHUD : NomaiVRModule<HelmetHUD.Behaviour, HelmetHUD.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Transform thrusterHUD;
            private Transform helmet;
            private HUDHelmetAnimator helmetAnimator;
            private static Behaviour instance;

            internal void Awake()
            {
                instance = this;

                FixCameraClipping();
                helmetAnimator = SetUpHelmetAnimator();
                var helmet = SetUpHelmet(helmetAnimator);
                CreateForwardIndicator(helmet);
                ReplaceHelmetModel(helmet);
                AdjustHudRenderer(helmetAnimator);
                var playerHud = GetPlayerHud(helmet);
                FixLockOnUI(playerHud);
                HideHudDuringDialogue(playerHud);
                SetHelmetScaleAndHUDOpacity();
                ModSettings.OnConfigChange += SetHelmetScaleAndHUDOpacity;
            }

            internal void OnDestroy()
            {
                ModSettings.OnConfigChange -= SetHelmetScaleAndHUDOpacity;
            }

            public void SetHelmetScaleAndHUDOpacity()
            {
                if (helmet)
                {
                    helmet.localScale = new Vector3(ModSettings.HudScale, ModSettings.HudScale, 1f) * 0.5f;
                    var uiColor = helmetAnimator._hudRenderer.material.color;
                    uiColor.a = ModSettings.HudOpacity * ModSettings.HudOpacity; //Squared for more drastic changes
                    helmetAnimator._hudRenderer.material.SetColor("_Color", uiColor);
                }
            }

            private void FixCameraClipping()
            {
                Camera.main.nearClipPlane = 0.05f;
            }

            private HUDHelmetAnimator SetUpHelmetAnimator()
            {
                var helmetAnimator = FindObjectOfType<HUDHelmetAnimator>();
                helmetAnimator._helmetOffsetSpring = new DampedSpring3D();
                return helmetAnimator;
            }

            private Transform SetUpHelmet(HUDHelmetAnimator helmetAnimator)
            {
                helmet = helmetAnimator.transform;
                helmet.localPosition = Vector3.forward * -0.07f;
                helmet.localScale = Vector3.one * 0.5f;
                helmet.gameObject.AddComponent<HelmetFollowCameraRotation>();
                return helmet;
            }

            private void CreateForwardIndicator(Transform helmet)
            {
                thrusterHUD = helmet.GetComponentInChildren<ThrustAndAttitudeIndicator>().transform;

                // Add a stronger line pointing forward in the thruster HUD
                var forwardIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                forwardIndicator.parent = thrusterHUD.Find("ThrusterArrows/Positive_Z");
                forwardIndicator.localPosition = Vector3.forward * 0.75f;
                forwardIndicator.localRotation = Quaternion.identity;
                forwardIndicator.localScale = new Vector3(0.05f, 0.05f, 1.5f);
                forwardIndicator.gameObject.layer = LayerMask.NameToLayer("HeadsUpDisplay");
                forwardIndicator.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Color");
                forwardIndicator.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
            }

            private void ReplaceHelmetModel(Transform helmet)
            {
                var helmetModelParent = helmet.Find("HelmetRoot/HelmetMesh/HUD_Helmet_v2");
                var helmetModel = Instantiate(AssetLoader.HelmetPrefab, helmetModelParent);
                LayerHelper.ChangeLayerRecursive(helmetModel, "VisibleToPlayer");
                Destroy(helmetModelParent.Find("Helmet").gameObject);
                Destroy(helmetModelParent.Find("HelmetFrame").gameObject);
                helmetModel.AddComponent<ConditionalRenderer>().GetShouldRender += () => ModSettings.ShowHelmet && Locator.GetPlayerSuit().IsWearingHelmet();
            }

            private void AdjustHudRenderer(HUDHelmetAnimator helmetAnimator)
            {
                var hudRenderer = helmetAnimator._hudRenderer.transform;
                hudRenderer.localScale = Vector3.one * 3.28f;
                hudRenderer.localPosition = new Vector3(-0.06f, -0.44f, 0.1f);
                hudRenderer.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = () => Locator.GetPlayerSuit().IsWearingHelmet();
                var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
                notifications.anchoredPosition = new Vector2(-200, -100);

                // HUD shader looks funky in stereo, so it needs to be replaced.
                var surfaceRenderer = hudRenderer.GetComponent<MeshRenderer>();
                surfaceRenderer.material.SetColor("_Color", new Color(1.5f, 1.5f, 1.5f, surfaceRenderer.material.color.a));
                MaterialHelper.MakeMaterialDrawOnTop(surfaceRenderer.material);
            }

            private void FixLockOnUI(Transform playerHud)
            {
                var lockOnCanvas = playerHud.Find("HelmetOffUI/HelmetOffLockOn").GetComponent<Canvas>();
                lockOnCanvas.planeDistance = 10;
            }

            private bool ShouldRenderHudParts()
            {
                return !ToolHelper.Swapper.IsInToolMode(ToolMode.Translator) && !PlayerState.InConversation();
            }

            private Transform GetPlayerHud(Transform helmet)
            {
                return helmet.Find("PlayerHUD");
            }

            private void HideHudDuringDialogue(Transform playerHud)
            {
                var uiCanvas = playerHud.Find("HelmetOnUI/UICanvas");
                foreach (Transform child in uiCanvas)
                {
                    if (child.name == "Notifications")
                    {
                        continue;
                    }
                    child.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = ShouldRenderHudParts;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ThrustAndAttitudeIndicator>(nameof(ThrustAndAttitudeIndicator.LateUpdate), nameof(FixThrusterHudRotation));
                    Postfix<HUDCamera>(nameof(HUDCamera.Awake), nameof(FixHudDistortion));
                    Postfix<HUDCamera>(nameof(HUDCamera.OnGraphicSettingsUpdated), nameof(FixHudDistortion));
                }

                private static void FixHudDistortion(Camera ____camera)
                {
                    ____camera.fieldOfView = 60;
                }

                private static void FixThrusterHudRotation()
                {
                    var rotation = instance.helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                    thrusterHUD.transform.rotation = rotation;
                }
            }
        }
    }
}
