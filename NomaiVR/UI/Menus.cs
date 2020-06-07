﻿using UnityEngine;
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
                var splashImage = titleMenu.Find("GamepadSplashCanvas/GamepadSplashImage").GetComponent<Image>(); ;
                splashImage.sprite = AssetLoader.SplashSprite;
            }

            private void ScreenCanvasesToWorld()
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (var canvas in canvases)
                {
                    // Filter out backdrop, to disable the background canvas during conversations.
                    var isBackdrop = canvas.name == "PauseBackdropCanvas";
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && !isBackdrop)
                    {
                        var scaler = canvas.GetComponent<CanvasScaler>();
                        if (scaler != null)
                        {
                            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                            scaler.scaleFactor = 1;
                            scaler.referencePixelsPerUnit = 100;
                        }
                        canvas.renderMode = RenderMode.WorldSpace;
                        canvas.transform.localScale = Vector3.one * 0.001f;
                        var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                        followTarget.target = SceneHelper.IsInGame() ? Locator.GetPlayerTransform() : Camera.main.transform.parent;
                        var z = SceneHelper.IsInGame() ? 1.5f : 2f;
                        var y = SceneHelper.IsInGame() ? 0.75f : 1f;
                        followTarget.localPosition = new Vector3(0, y, z);
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