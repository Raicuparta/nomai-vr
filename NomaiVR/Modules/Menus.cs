using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NomaiVR {
    public class Menus: MonoBehaviour {
        public bool isInGame;
        static float _farClipPlane = -1;
        static int _cullingMask;

        // List all the canvas elements that need to be moved to world space during gameplay.
        static readonly CanvasInfo[] _canvasInfos = {
            new CanvasInfo("PauseMenu"),
            new CanvasInfo("CanvasMarkerManager", 0.0005f),
            new CanvasInfo("DialogueCanvas"),
            new CanvasInfo("ScreenPromptCanvas", 0.0015f),
        };

        void Start () {
            NomaiVR.Log("Start Menus");

            // Make UI elements draw on top of everything.
            Canvas.GetDefaultCanvasMaterial().SetInt("unity_GUIZTestMode", (int) CompareFunction.Always);

            FixMainMenuCanvas();
            //FixAllCanvases();

            if (isInGame) {
                FixGameCanvases(new[] {
                    new CanvasInfo("PauseMenu", 0.0005f),
                    new CanvasInfo("DialogueCanvas"),
                    new CanvasInfo("ScreenPromptCanvas", 0.0015f),
                });
                //FixAllCanvases();

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

        public static void Reset () {
            _farClipPlane = -1;
        }

        void OnWakeUp () {
            NomaiVR.Log("wake up freeman");
            Camera.main.cullingMask = _cullingMask;
            Camera.main.farClipPlane = _farClipPlane;
        }

        void MoveCanvasToWorldSpace (CanvasInfo canvasInfo) {
            GameObject canvas = GameObject.Find(canvasInfo.name);

            if (canvas == null) {
                NomaiVR.Log("Couldn't find canvas with name: " + canvasInfo.name);
                return;
            }

            Canvas[] subCanvases = canvas.GetComponentsInChildren<Canvas>();

            foreach (Canvas subCanvas in subCanvases) {
                subCanvas.renderMode = RenderMode.WorldSpace;
                subCanvas.transform.localPosition = Vector3.zero;
                subCanvas.transform.localRotation = Quaternion.identity;
                subCanvas.transform.localScale = Vector3.one;
            }

            canvas.transform.parent = Common.MainCamera.transform;
            canvas.transform.localPosition = canvasInfo.offset;
            canvas.transform.localEulerAngles = new Vector3(0, 0, 0);
            canvas.transform.localScale = Vector3.one * canvasInfo.scale;

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

        void FixMainMenuCanvas () {
            MoveCanvasToWorldSpace(new CanvasInfo("TitleMenu", 0.0005f));
        }

        void FixGameCanvases (CanvasInfo[] canvasInfos) {
            foreach (CanvasInfo canvasInfo in canvasInfos) {
                MoveCanvasToWorldSpace(canvasInfo);
            }
        }

        void FixAllCanvases () {
            var canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases) {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = Camera.main;
                    canvas.planeDistance = 5;
                }
            }
        }

        protected class CanvasInfo {
            public string name;
            public Vector3 offset;
            public float scale;
            const float _defaultScale = 0.001f;

            public CanvasInfo (string _name, Vector3 _offset, float _scale = _defaultScale) {
                name = _name;
                offset = _offset;
                scale = _scale;
            }

            public CanvasInfo (string _name, float _scale = _defaultScale) {
                name = _name;
                offset = new Vector3(0, 0, 1);
                scale = _scale;
            }
        }
    }
}