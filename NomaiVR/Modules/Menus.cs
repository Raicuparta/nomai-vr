using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Menus : MonoBehaviour
    {
        Camera _camera;

        // List all the canvas elements that need to be moved to world space during gameplay.
        static readonly CanvasInfo[] _canvasInfos = {
            new CanvasInfo("PauseMenu", 0.0005f),
            new CanvasInfo("DialogueCanvas"),
            new CanvasInfo("ScreenPromptCanvas", 0.0015f),
            
            // This is the canvas that renders the lights in Dark Bramble.
            // Not sure how to make it look nice in VR.
            // new CanvasInfo("FogLightCanvas"),
        };

        void Start() {
            NomaiVR.Log("Start Menus");

            SceneManager.sceneLoaded += OnSceneLoaded;

            FixMainMenuCanvas();

            // Make UI elements draw on top of everything.
            Canvas.GetDefaultCanvasMaterial().SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);

        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            _camera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
            FixGameCanvases();
        }

        void MoveCanvasToWorldSpace(CanvasInfo canvasInfo) {
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

            canvas.transform.parent = _camera.transform;
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

        void FixMainMenuCanvas() {
            _camera = GameObject.FindObjectOfType<Camera>();
            MoveCanvasToWorldSpace(new CanvasInfo("TitleMenu", 0.0005f));
        }

        void FixGameCanvases() {
            foreach (CanvasInfo canvasInfo in _canvasInfos) {
                MoveCanvasToWorldSpace(canvasInfo);
            }
        }
        protected class CanvasInfo
        {
            public string name;
            public Vector3 offset;
            public float scale;
            const float _defaultScale = 0.001f;

            public CanvasInfo(string _name, Vector3 _offset, float _scale = _defaultScale) {
                name = _name;
                offset = _offset;
                scale = _scale;
            }

            public CanvasInfo(string _name, float _scale = _defaultScale) {
                name = _name;
                offset = new Vector3(0, 0, 1);
                scale = _scale;
            }
        }
    }
}