using OWML.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NomaiVR {
    public class Menus: MonoBehaviour {
        void Awake () {
            NomaiVR.Helper.Events.Subscribe<CanvasMarkerManager>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;
        }

        void Start () {
            NomaiVR.Log("Start Menus");

            // Make UI elements draw on top of everything.
            Canvas.GetDefaultCanvasMaterial().SetInt("unity_GUIZTestMode", (int) CompareFunction.Always);

            var scene = LoadManager.GetCurrentScene();

            if (scene == OWScene.SolarSystem) {
                // Make sleep timer canvas visible while eyes closed.
                Locator.GetUIStyleManager().transform.Find("SleepTimerCanvas").gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
            } else if (scene == OWScene.TitleScreen) {
                var animatedTitle = GameObject.Find("TitleCanvasHack").GetComponent<Canvas>();
                animatedTitle.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            ScreenCanvasesToWorld();
        }

        private void OnEvent (MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(CanvasMarkerManager) && ev == Events.AfterStart) {
                var canvas = GameObject.Find("CanvasMarkerManager").GetComponent<Canvas>();
                canvas.planeDistance = 5;
            }
        }

        void ScreenCanvasesToWorld () {
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases) {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.name != "PauseBackdropCanvas") {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                    canvas.transform.rotation = Camera.main.transform.rotation;
                    canvas.transform.localScale *= 0.001f;
                    var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                    followTarget.positionSmoothTime = 0.2f;
                    followTarget.rotationSmoothTime = 0.1f;
                    followTarget.target = Camera.main.transform;
                    followTarget.localPosition = Vector3.forward;

                    // Masks are used for hiding the overflowing elements in scrollable menus.
                    // Apparently masks change the material of the canvas element being masked,
                    // and I'm not sure how to change unity_GUIZTestMode there.
                    // So for now I'm disabling the mask completely, which breaks some menus.
                    var masks = canvas.GetComponentsInChildren<Mask>(true);
                    foreach (var mask in masks) {
                        mask.enabled = false;
                        mask.graphic.enabled = false;
                    }
                }
            }
        }
    }
}