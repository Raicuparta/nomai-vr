using OWML.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NomaiVR {
    public class Menus: MonoBehaviour {
        public bool isInGame;
        static float _farClipPlane = -1;
        static int _cullingMask;

        void Awake () {
            NomaiVR.Helper.Events.Subscribe<CanvasMarkerManager>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnEvent;
        }

        void Start () {
            NomaiVR.Log("Start Menus");

            // Make UI elements draw on top of everything.
            Canvas.GetDefaultCanvasMaterial().SetInt("unity_GUIZTestMode", (int) CompareFunction.Always);

            ScreenCanvasesToWorld();

            if (SceneManager.GetActiveScene().name == "SolarSystem") {
                GlobalMessenger.AddListener("WakeUp", OnWakeUp);

                if (_farClipPlane == -1) {
                    _cullingMask = Camera.main.cullingMask;
                    _farClipPlane = Camera.main.farClipPlane;
                    Locator.GetPlayerCamera().postProcessingSettings.eyeMaskEnabled = false;
                    Camera.main.cullingMask = (1 << 5);
                    Camera.main.farClipPlane = 5;
                }
            }
        }

        private void OnEvent (MonoBehaviour behaviour, Events ev) {
            if (behaviour.GetType() == typeof(CanvasMarkerManager) && ev == Events.AfterStart) {
                var canvas = GameObject.Find("CanvasMarkerManager").GetComponent<Canvas>();
                canvas.planeDistance = 5;
            }
        }

        public static void Reset () {
            _farClipPlane = -1;
        }

        void OnWakeUp () {
            Camera.main.cullingMask = _cullingMask;
            Camera.main.farClipPlane = _farClipPlane;
        }

        void ScreenCanvasesToWorld () {
            var canvases = FindObjectsOfType<Canvas>();
            NomaiVR.Log("found", canvases.Length.ToString(), "canvases");
            foreach (var canvas in canvases) {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    canvas.renderMode = RenderMode.WorldSpace;
                    //canvas.transform.parent = Camera.main.transform;
                    canvas.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
                    canvas.transform.localRotation = Camera.main.transform.rotation;
                    canvas.transform.localScale *= 0.001f;
                    var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
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