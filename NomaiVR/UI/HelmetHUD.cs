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
            private Transform _helmet;
            private static Behaviour _instance;

            internal void Awake()
            {
                _instance = this;

                FixCameraClipping();
                var helmetAnimator = SetUpHelmetAnimator();
                var helmet = SetUpHelmet(helmetAnimator);
                CreateForwardIndicator(helmet);
                ReplaceHelmetModel(helmet);
                AdjustHudRenderer(helmetAnimator);
                var playerHud = GetPlayerHud(helmet);
                FixLockOnUI(playerHud);
                HideHudDuringDialogue(playerHud);
                SetHelmetScale();
                ModSettings.OnConfigChange += SetHelmetScale;
            }

            public static void SetHelmetScale()
            {
                var helmet = _instance?._helmet;
                if (!helmet)
                {
                    return;
                }
                helmet.localScale = new Vector3(ModSettings.HudScale, ModSettings.HudScale, 1f) * 0.5f;
            }

            private void FixCameraClipping()
            {
                Camera.main.nearClipPlane = 0.01f;
            }

            private HUDHelmetAnimator SetUpHelmetAnimator()
            {
                var helmetAnimator = FindObjectOfType<HUDHelmetAnimator>();
                helmetAnimator.SetValue("_helmetOffsetSpring", new DampedSpring3D());
                return helmetAnimator;
            }

            private Transform SetUpHelmet(HUDHelmetAnimator helmetAnimator)
            {
                _helmet = helmetAnimator.transform;
                _helmet.localPosition = Vector3.forward * -0.07f;
                _helmet.localScale = Vector3.one * 0.5f;
                _helmet.gameObject.AddComponent<SmoothFollowCameraRotation>();
                return _helmet;
            }

            private void CreateForwardIndicator(Transform helmet)
            {
                _thrusterHUD = helmet.GetComponentInChildren<ThrustAndAttitudeIndicator>().transform;

                // Add a stronger line pointing forward in the thruster HUD
                var forwardIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                forwardIndicator.parent = _thrusterHUD.Find("ThrusterArrows/Positive_Z");
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
                helmetModel.AddComponent<ConditionalRenderer>().getShouldRender += () => ModSettings.ShowHelmet && Locator.GetPlayerSuit().IsWearingHelmet();
            }

            private void AdjustHudRenderer(HUDHelmetAnimator helmetAnimator)
            {
                var hudRenderer = helmetAnimator.GetValue<MeshRenderer>("_hudRenderer").transform;
                hudRenderer.localScale = Vector3.one * 3.28f;
                hudRenderer.localPosition = new Vector3(-0.06f, -0.44f, 0.1f);
                hudRenderer.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () => Locator.GetPlayerSuit().IsWearingHelmet();
                var notifications = FindObjectOfType<SuitNotificationDisplay>().GetComponent<RectTransform>();
                notifications.anchoredPosition = new Vector2(-200, -100);

                // HUD shader looks funky in stereo, so it needs to be replaced.
                var surfaceRenderer = hudRenderer.GetComponent<MeshRenderer>();
                surfaceRenderer.material.SetColor("_Color", new Color(1.5f, 1.5f, 1.5f, 1));
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
                    child.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = ShouldRenderHudParts;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ThrustAndAttitudeIndicator>("LateUpdate", nameof(FixThrusterHudRotation));
                    Postfix<HUDCamera>("Awake", nameof(FixHudDistortion));
                }

                private static void FixHudDistortion(Camera ____camera)
                {
                    ____camera.fieldOfView = 60;
                }

                private static void FixThrusterHudRotation()
                {
                    var rotation = _instance._helmet.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
                    _thrusterHUD.transform.rotation = rotation;
                }
            }
        }
    }
}
