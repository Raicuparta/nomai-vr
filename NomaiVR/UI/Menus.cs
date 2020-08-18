using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class Menus : NomaiVRModule<Menus.Behaviour, Menus.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private static bool _shouldRenderStarLogos;
            private static readonly List<Canvas> patchedCanvases = new List<Canvas>();
            private Camera _flashbackCamera;
            private Transform _flashbackCameraParent;
            private GameObject _canvasParent;

            internal void Start()
            {
                SetUpCanvasParent();
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
                }

                ScreenCanvasesToWorld();
            }

            internal void Update()
            {
                if (_canvasParent.activeSelf && !IsMenuInteractionAllowed())
                {
                    _canvasParent.SetActive(false);
                    return;
                }
                if (!_canvasParent.activeSelf && IsMenuInteractionAllowed())
                {
                    _canvasParent.SetActive(true);
                    return;
                }
            }

            private bool IsMenuInteractionAllowed()
            {
                return OWTime.IsPaused() || SceneHelper.IsInTitle();
            }

            private void SetUpCanvasParent()
            {
                _canvasParent = new GameObject();
            }

            private void SetUpFlashbackCameraParent()
            {
                _flashbackCamera = FindObjectOfType<Flashback>().GetComponent<Camera>();
                if (!_flashbackCameraParent)
                {
                    _flashbackCameraParent = new GameObject().transform;
                }
                if (_flashbackCamera.transform.parent == null)
                {
                    _flashbackCamera.transform.SetParent(_flashbackCameraParent);
                }
                // FlashbackCamera objects starts really far from Vector3.zero.
                // Far enough to cause floating point imprecision glitches.
                // Usually the game would move the camera to Vector3.zero, but camera movement isn't possible in VR.
                // So we need to apply the inverse position to make it move to Vector3.zero.
                _flashbackCameraParent.position = _flashbackCamera.transform.localPosition * -1;
            }

            private static void FixStarLogo(string objectName)
            {
                var logo = GameObject.Find(objectName).transform;
                logo.localRotation *= Quaternion.Euler(30, 0, 0);
                logo.gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () => _shouldRenderStarLogos;
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
                canvas.transform.parent = _canvasParent.transform;
                var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                if (SceneHelper.IsInGame())
                {
                    followTarget.target = Locator.GetPlayerTransform();
                    followTarget.localPosition = new Vector3(0, 0.75f, 1.5f);
                }
                else if (SceneHelper.IsInTitle())
                {
                    followTarget.target = Camera.main.transform.parent;
                    followTarget.localPosition = new Vector3(-0.2f, 1.3f, 2f);
                }
                else
                {
                    followTarget.target = Camera.main.transform;
                    followTarget.localPosition = new Vector3(0, 0, 2f);
                    followTarget.positionSmoothTime = 0.5f;
                    followTarget.rotationSmoothTime = 0.5f;
                }
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
                var selectables = canvas.GetComponentsInChildren<TooltipSelectable>(true);
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

            private bool IsPauseBackdopCanvas(Canvas canvas)
            {
                return canvas.name == "PauseBackdropCanvas";
            }

            private bool IsOwmlModConfigMenuCanvas(Canvas canvas)
            {
                // Hack! If OWML ever changes this name, this check needs to be fixed.
                return canvas.name == "KeyboardRebindingCanvas(Clone)";
            }

            private void SetUpDeathTextCanvas(Canvas canvas)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = _flashbackCamera;
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
                    if (IsPauseBackdopCanvas(canvas))
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

                    if (isScreenSpaceOverlay || isPatched)
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
                    Postfix<ProfileMenuManager>("PopulateProfiles", nameof(PostPopulateProfiles));
                    Postfix<CanvasMarkerManager>("Start", nameof(PostMarkerManagerStart));
                    Postfix<TitleScreenAnimation>("FadeInMusic", nameof(PostTitleScreenFadeInMusic));
                    Postfix<PopupMenu>("SetUpPopupCommands", nameof(PostSetPopupCommands));
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