using OWML.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Menus : NomaiVRModule<Menus.Behaviour, Menus.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private void Awake()
            {
                NomaiVR.Helper.Events.Subscribe<CanvasMarkerManager>(Events.AfterStart);
                NomaiVR.Helper.Events.OnEvent += OnEvent;
            }

            private void Start()
            {
                // Make UI elements draw on top of everything.
                Canvas.GetDefaultCanvasMaterial().SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);

                var scene = LoadManager.GetCurrentScene();

                if (scene == OWScene.SolarSystem)
                {
                    // Make sleep timer canvas visible while eyes closed.
                    Locator.GetUIStyleManager().transform.Find("SleepTimerCanvas").gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
                }
                else if (scene == OWScene.TitleScreen)
                {
                    var animatedTitle = GameObject.Find("TitleCanvasHack").GetComponent<Canvas>();
                    animatedTitle.renderMode = RenderMode.ScreenSpaceOverlay;

                    var animatedTitleChild = animatedTitle.transform.GetChild(0).GetComponent<RectTransform>();
                    animatedTitleChild.anchorMax = Vector2.one * 0.5f;
                    animatedTitleChild.anchorMin = Vector2.one * 0.5f;

                    var mainMenu = GameObject.Find("TitleLayoutGroup").GetComponent<RectTransform>();
                    mainMenu.position = Vector3.zero;

                    // Cant't get the footer to look good, so I'm hiding it.
                    GameObject.Find("FooterBlock").SetActive(false);

                    // Make the camera start looking forward instead of some random direction.
                    //var cameraSocket = GameObject.Find("CameraSocket").transform;
                    //cameraSocket.rotation = Quaternion.identity;
                }

                ScreenCanvasesToWorld();
            }

            private void OnEvent(MonoBehaviour behaviour, Events ev)
            {
                if (behaviour.GetType() == typeof(CanvasMarkerManager) && ev == Events.AfterStart)
                {
                    var canvas = GameObject.Find("CanvasMarkerManager").GetComponent<Canvas>();
                    canvas.planeDistance = 5;
                }
            }

            private void ScreenCanvasesToWorld()
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.name != "PauseBackdropCanvas")
                    {
                        canvas.renderMode = RenderMode.WorldSpace;
                        canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                        canvas.transform.rotation = Camera.main.transform.rotation;
                        canvas.transform.localScale *= 0.001f;
                        var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                        followTarget.positionSmoothTime = 0.2f;
                        followTarget.rotationSmoothTime = 0.1f;
                        followTarget.target = SceneHelper.IsInGame() ? Locator.GetPlayerTransform() : Camera.main.transform.parent;
                        var z = SceneHelper.IsInGame() ? 1f : 1.5f;
                        var y = SceneHelper.IsInGame() ? 0.5f : 1f;
                        followTarget.localPosition = new Vector3(0, y, z);

                        // Masks are used for hiding the overflowing elements in scrollable menus.
                        // Apparently masks change the material of the canvas element being masked,
                        // and I'm not sure how to change unity_GUIZTestMode there.
                        // So for now I'm disabling the mask completely, which breaks some menus.
                        var masks = canvas.GetComponentsInChildren<Mask>(true);
                        foreach (var mask in masks)
                        {
                            mask.enabled = false;
                            mask.graphic.enabled = false;
                        }
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ProfileMenuManager>("PopulateProfiles", typeof(Patch), nameof(PostPopulateProfiles));

                    // Make options menu background color transparent,
                    // to prevent obscuring the laser
                    NomaiVR.Post<TabbedOptionMenu>("Initialize", typeof(Patch), nameof(PostOptionMenuInitialize));
                }

                private static void PostOptionMenuInitialize(TabbedOptionMenu __instance)
                {
                    // TODO: fix this in main menu.
                    //var displayPanel = __instance.transform.Find("OptionsDisplayPanel");
                    //displayPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.78f);
                    //displayPanel.Find("Background").gameObject.SetActive(false);

                    //var tabsBackground = __instance.transform.Find("Tabs/Background");
                    //tabsBackground.GetComponent<Image>().color = new Color(0, 0, 0, 0.78f);
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
            }
        }
    }
}