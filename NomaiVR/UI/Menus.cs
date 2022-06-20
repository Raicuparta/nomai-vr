using System.Collections.Generic;
using System.Linq;
using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.UI
{
    internal class Menus : NomaiVRModule<Menus.Behaviour, Menus.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private static bool shouldRenderStarLogos;
            private static readonly List<Canvas> patchedCanvases = new List<Canvas>();
            private static readonly string[] ignoredCanvases =
            {
                "LoadManagerFadeCanvas", 
                "PauseBackdropCanvas", 
                "Reticule", 
                "com.sinai.unityexplorer_Root" // Unity Explorer
            };
            private readonly List<GameObject> canvasObjectsToHide = new List<GameObject>();
            private Camera flashbackCamera;
            private Transform flashbackCameraParent;
            private bool isCanvasObjectsActive;
            private bool fadeInLogo;

            internal void Start()
            {
                if (SceneHelper.IsInGame())
                {
                    SetUpFlashbackCameraParent();
                }

                if (SceneHelper.IsInSolarSystem())
                {
                    FixSleepTimerCanvas();
                }

                if (SceneHelper.IsInTitle())
                {
                    FixTitleMenuCanvases();
                    FixStarLogos();
                    FixOuterWildsLogo();
                    StopCameraRotation();
                }

                ScreenCanvasesToWorld();

                ModSettings.OnConfigChange += ModSettings_OnConfigChange;
            }

            internal void OnDestroy()
            {
                ModSettings.OnConfigChange -= ModSettings_OnConfigChange;
            }

            private void ModSettings_OnConfigChange()
            {
                if (Locator.GetMarkerManager() != null)
                {
                    foreach (CanvasMarker marker in Locator.GetMarkerManager()._nonFogMarkers)
                    {
                        MaterialHelper.SetCanvasAlpha(marker._canvas, ModSettings.MarkersOpacity * ModSettings.MarkersOpacity);
                    }
                }
            }

            internal void Update()
            {
                UpdateCanvasObjectsActive();
            }

            private void UpdateCanvasObjectsActive()
            {
                if (isCanvasObjectsActive && !IsMenuInteractionAllowed())
                {
                    SetCanvasObjectsActive(false);
                    return;
                }
                if (!isCanvasObjectsActive && IsMenuInteractionAllowed())
                {
                    SetCanvasObjectsActive(true);
                    return;
                }
            }

            private void SetCanvasObjectsActive(bool active)
            {
                canvasObjectsToHide.ForEach(canvasObject => canvasObject.SetActive(active));
                isCanvasObjectsActive = active;
            }

            private bool IsMenuInteractionAllowed()
            {
                return OWTime.IsPaused(OWTime.PauseType.Menu) || !SceneHelper.IsInGame() || PlayerState.IsSleepingAtCampfire();
            }

            private void SetUpFlashbackCameraParent()
            {
                flashbackCamera = FindObjectOfType<Flashback>().GetComponent<Camera>();
                if (!flashbackCameraParent)
                {
                    flashbackCameraParent = new GameObject("VrFlashbackCameraWrapper").transform;
                }
                if (flashbackCamera.transform.parent == null)
                {
                    flashbackCamera.transform.SetParent(flashbackCameraParent);
                }
                // FlashbackCamera objects starts really far from Vector3.zero.
                // Far enough to cause floating point imprecision glitches.
                // Usually the game would move the camera to Vector3.zero, but camera movement isn't possible in VR.
                // So we need to apply the inverse position to make it move to Vector3.zero.
                flashbackCameraParent.position = flashbackCamera.transform.localPosition * -1;
            }

            private static void StopCameraRotation()
            {
                var rotateTransformComponent = GameObject.Find("Scene/Background").GetComponent<RotateTransform>();
                Destroy(rotateTransformComponent);
            }

            private void FixOuterWildsLogo()
            {
                var logoParentTranform = GameObject.Find("TitleCanvasHack/TitleLayoutGroup").transform;
                var canvasHack = logoParentTranform.parent;
                Destroy(canvasHack.GetComponent<CanvasScaler>()); //Remove Canvas Scaler
                canvasHack.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                logoParentTranform.localPosition = Vector3.left * 400;
                fadeInLogo = false;

                LayerHelper.ChangeLayerRecursive(logoParentTranform.gameObject, "VisibleToPlayer");

                //Logo Fade-In Animation
                var logoFader = logoParentTranform.gameObject.AddComponent<TitleMenuLogoFader>();
                var logoAnimator = logoParentTranform.GetComponentInChildren<Animator>();
                logoFader.BeginFade(1f, 3, () => fadeInLogo, x => Mathf.Pow(x, 3), true); //FIXME: Broke, too fast

                FindObjectOfType<TitleAnimationController>().OnTitleLogoAnimationComplete += () =>
                {
                    canvasHack.localScale = Vector3.one * 0.126f;
                    canvasHack.position = new Vector3(17.344f, 136.154f, 10.499f);
                    canvasHack.rotation = Quaternion.Euler(342.012f, 116.613f, 325.473f);
                    fadeInLogo = true;
                };
            }

            private static void FixStarLogo(string objectName)
            {
                var logo = GameObject.Find(objectName).transform;
                logo.localRotation *= Quaternion.Euler(30, 0, 0);
                logo.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = () => shouldRenderStarLogos;
            }

            private static void FixStarLogos()
            {
                FixStarLogo("StarfieldMobius_Pivot");
                FixStarLogo("StarfieldAnnapurna_Pivot");
            }

            private static void FixSleepTimerCanvas()
            {
                // Make sleep timer canvas visible while eyes closed.
                Locator.GetUIStyleManager().transform.Find("SleepTimerCanvas").gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
            }

            private static void FixTitleMenuCanvases()
            {
                var titleMenu = GameObject.Find("TitleMenu").transform;
                var titleCanvas = titleMenu.Find("TitleCanvas");

                // Hide the main menu while other menus are open,
                // to prevent selecting with laser.
                titleCanvas.Find("TitleLayoutGroup").gameObject.AddComponent<ConditionalRenderer>().GetShouldRender = () =>
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
                canvasObjectsToHide.Add(canvas.gameObject);
                var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                followTarget.updateType = FollowTarget.UpdateType.PreCull;
                if (SceneHelper.IsInGame())
                {
                    followTarget.Target = Locator.GetPlayerTransform();
                    followTarget.LocalPosition = new Vector3(0, 0.75f, 1.5f);
                }
                else if (SceneHelper.IsInTitle())
                {
                    followTarget.Target = Camera.main.transform.parent;
                    followTarget.LocalPosition = new Vector3(-0.2f, 1.3f, 2f);
                }
                else
                {
                    followTarget.Target = Camera.main.transform;
                    followTarget.LocalPosition = new Vector3(0, 0, 2f);
                    followTarget.PositionSmoothTime = 0.5f;
                    followTarget.RotationSmoothTime = 0.5f;
                }

                canvas.gameObject.AddComponent<DestroyObserver>().OnDestroyed += () =>
                {
                    canvasObjectsToHide.Remove(canvas.gameObject);
                };
            }

            private static void AdjustScaler(Canvas canvas)
            {
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    scaler.scaleFactor = 1;
                    scaler.referencePixelsPerUnit = 100;
                }
                if (SceneHelper.IsInTitle())
                {
                    canvas.transform.localScale = Vector3.one * 0.0015f;
                }
                else
                {
                    canvas.transform.localScale = Vector3.one * 0.001f;
                }
            }

            // Mod config buttons generated by OWML get broken,
            // this workaround fixes their position;
            private static void AdjustModConfigButtons(Canvas canvas)
            {
                var selectables = canvas.GetComponentsInChildren<TooltipDisplay>(true);
                foreach (var selectable in selectables)
                {
                    selectable.transform.localPosition = new Vector3(851.5f, -35f, 0);
                    selectable.transform.localRotation = Quaternion.identity;
                }
            }

            private bool IsDeathTextCanvas(Canvas canvas)
            {
                return canvas.name == "Canvas_Text";
            }

            private bool IsIgnoredCanvas(Canvas canvas)
            {
                return ignoredCanvases.Contains(canvas.name);
            }

            private bool IsOwmlModConfigMenuCanvas(Canvas canvas)
            {
                // Hack! If OWML ever changes this name, this check needs to be fixed.
                return canvas.name == "KeyboardRebindingCanvas(Clone)";
            }

            private void SetUpDeathTextCanvas(Canvas canvas)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = flashbackCamera;
                canvas.planeDistance = 15f;
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 0.2f;
                scaler.referencePixelsPerUnit = 100;
            }

            private void ScreenCanvasesToWorld()
            {
                var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (IsIgnoredCanvas(canvas))
                    {
                        continue;
                    }

                    if (IsDeathTextCanvas(canvas))
                    {
                        SetUpDeathTextCanvas(canvas);
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

                    if ((isScreenSpaceOverlay || isPatched) && !SceneHelper.IsInPostCredits())
                    {
                        AddFollowTarget(canvas);
                    }

                    if (isPatched && IsOwmlModConfigMenuCanvas(canvas))
                    {
                        AdjustModConfigButtons(canvas);
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ProfileMenuManager>(nameof(ProfileMenuManager.PopulateProfiles), nameof(PostPopulateProfiles));
                    Postfix<CanvasMarkerManager>(nameof(CanvasMarkerManager.Start), nameof(PostMarkerManagerStart));
                    Postfix<TitleScreenAnimation>(nameof(TitleScreenAnimation.FadeInMusic), nameof(PostTitleScreenFadeInMusic));
                    Postfix<PopupMenu>(nameof(PopupMenu.SetUpPopupCommands), nameof(PostSetPopupCommands));
                }

                private static void PostSetPopupCommands(IInputCommands okCommand, ref IInputCommands ____okCommand)
                {
                    if (okCommand == InputLibrary.select)
                    {
                        ____okCommand = InputLibrary.confirm;
                    }
                }

                private static void PostTitleScreenFadeInMusic()
                {
                    shouldRenderStarLogos = true;
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

                    foreach (CanvasMarker marker in __instance._nonFogMarkers)
                    {
                        MaterialHelper.SetCanvasAlpha(marker._canvas, ModSettings.MarkersOpacity * ModSettings.MarkersOpacity);
                    }
                }
            }
        }
    }
}