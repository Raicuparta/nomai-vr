using Harmony;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Menus : NomaiVRModule<Menus.Behaviour, Menus.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private static bool _shouldRenderStarLogos;
            private static List<Canvas> patchedCanvases = new List<Canvas>();

            private void Start()
            {
                var scene = LoadManager.GetCurrentScene();

                if (scene == OWScene.SolarSystem)
                {
                    FixSleepTimerCanvas();
                }
                else if (scene == OWScene.TitleScreen)
                {
                    FixTitleMenuCanvases();
                    FixStarLogos();

                }
                ScreenCanvasesToWorld();
            }

            private void FixStarLogo(string objectName)
            {
                var logo = GameObject.Find(objectName).transform;
                logo.localRotation *= Quaternion.Euler(30, 0, 0);
                logo.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () => _shouldRenderStarLogos;
            }

            private void FixStarLogos()
            {
                FixStarLogo("StarfieldMobius_Pivot");
                FixStarLogo("StarfieldAnnapurna_Pivot");
            }

            private void FixSleepTimerCanvas()
            {
                // Make sleep timer canvas visible while eyes closed.
                Locator.GetUIStyleManager().transform.Find("SleepTimerCanvas").gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
            }

            private void FixTitleMenuCanvases()
            {
                var titleMenu = GameObject.Find("TitleMenu").transform;

                // Hide the main menu while other menus are open,
                // to prevent selecting with laser.
                var titleCanvas = titleMenu.Find("TitleCanvas");
                titleMenu.Find("TitleCanvas").gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () =>
                    MenuStackManager.SharedInstance.GetMenuCount() == 0;

                // Cant't get the footer to look good, so I'm hiding it.
                titleCanvas.Find("FooterBlock").gameObject.SetActive(false);

                // Replace splash image.
                var splashCanvas = titleMenu.Find("GamepadSplashCanvas");
                var splashText = splashCanvas.Find("Text").GetComponent<Text>();
                splashText.text = "<color=orange>NomaiVR</color> requires VR controllers...";
                var splashImage = splashCanvas.Find("GamepadSplashImage").GetComponent<Image>(); ;
                splashImage.sprite = AssetLoader.SplashSprite;
            }

            private void AddFollowTarget(Canvas canvas)
            {
                var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                followTarget.target = SceneHelper.IsInGame() ? Locator.GetPlayerTransform() : Camera.main.transform.parent;
                var z = SceneHelper.IsInGame() ? 1.5f : 2f;
                var y = SceneHelper.IsInGame() ? 0.75f : 1f;
                followTarget.localPosition = new Vector3(0, y, z);
            }

            private void AdjustScaler(Canvas canvas)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    scaler.scaleFactor = 1;
                    scaler.referencePixelsPerUnit = 100;
                }
                canvas.transform.localScale = Vector3.one * 0.001f;
            }

            private void ScreenCanvasesToWorld()
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (var canvas in canvases)
                {
                    // Filter out backdrop, to disable the background canvas during conversations.
                    if (canvas.name == "PauseBackdropCanvas")
                    {
                        continue;
                    }

                    var isScreenSpaceOverlay = canvas.renderMode == RenderMode.ScreenSpaceOverlay;
                    var isPatched = patchedCanvases.Contains(canvas);

                    if (isScreenSpaceOverlay)
                    {
                        canvas.renderMode = RenderMode.WorldSpace;
                        AdjustScaler(canvas);
                        patchedCanvases.Add(canvas);
                    }

                    if (isScreenSpaceOverlay || isPatched)
                    {
                        AddFollowTarget(canvas);
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ProfileMenuManager>("PopulateProfiles", typeof(Patch), nameof(PostPopulateProfiles));
                    NomaiVR.Post<CanvasMarkerManager>("Start", typeof(Patch), nameof(PostMarkerManagerStart));
                    NomaiVR.Post<TitleScreenAnimation>("FadeInMusic", typeof(Patch), nameof(PostTitleScreenFadeInMusic));
                    NomaiVR.Post<PopupMenu>("SetUpPopupCommands", typeof(Patch), nameof(PostSetPopupCommands));
                    //NomaiVR.Post<Canvas>("OnEnable", typeof(Patch), nameof(PostCanvasAwake));
                }

                private static void PostCanvasAwake()
                {
                    NomaiVR.Log("PostCanvasAwake");
                }

                private static void PostSetPopupCommands(SingleAxisCommand okCommand, ref SingleAxisCommand ____okCommand)
                {
                    if (okCommand == InputLibrary.select)
                    {
                        ____okCommand = InputLibrary.confirm;
                    }
                }

                private static void PostTitleScreenFadeInMusic()
                {
                    _shouldRenderStarLogos = true;
                }

                private static void PostPopulateProfiles(GameObject ____profileListRoot)
                {
                    foreach (Transform child in ____profileListRoot.transform)
                    {
                        child.localPosition = Vector3.zero;
                        child.localRotation = Quaternion.identity;
                        child.localScale = Vector3.one;
                    }
                }

                private static void PostMarkerManagerStart(CanvasMarkerManager __instance)
                {
                    __instance.GetComponent<Canvas>().planeDistance = 5;
                }
            }
        }
    }
}