using UnityEngine;
using UnityEngine.SceneManagement;

namespace OWML.NomaiVR
{
    public class Menus : MonoBehaviour
    {
        Camera _camera;

        // List all the canvas elements that need to be moved to world space during gameplay.
        static readonly CanvasInfo[] _canvasInfos = {
            new CanvasInfo("PauseMenu", new Vector3(-0.6f, -0.5f, 1)),
            new CanvasInfo("DialogueCanvas"),
            new CanvasInfo("ScreenPromptCanvas"),
        };

        void Start() {
            NomaiVR.Log("Start Menus");

            SceneManager.sceneLoaded += OnSceneLoaded;

            // Main menu camera
            _camera = GameObject.FindObjectOfType<Camera>();
            FixMainMenuCanvas();
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            _camera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
            FixGameCanvases();
        }

        void MoveAllCanvasesToWorldSpace() {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

            foreach (Canvas canvas in canvases) {
                NomaiVR.Log("found " + canvas.name);
                canvas.renderMode = RenderMode.WorldSpace;
                Transform originalParent = canvas.transform.parent;
                canvas.transform.parent = Object.FindObjectOfType<Camera>().transform;
                canvas.transform.localPosition = new Vector3(0, 0, 1);
                canvas.transform.localRotation = new Quaternion(0, 0, 0, 1);
                canvas.transform.localScale = Vector3.one * 0.0005f;
            }
        }

        void MoveCanvasToWorldSpace(CanvasInfo canvasInfo) {
            GameObject canvas = GameObject.Find(canvasInfo.name);

            if (canvas == null) {
                NomaiVR.Log("Couldn't find canvas with name: " + canvasInfo.name);
                return;
            }

            Canvas[] subCanvases = canvas.GetComponentsInChildren<Canvas>();
            NomaiVR.Log("subcanvases: " + subCanvases.Length);

            foreach (Canvas subCanvas in subCanvases) {
                subCanvas.renderMode = RenderMode.WorldSpace;
            }

            canvas.transform.parent = _camera.transform;
            canvas.transform.localPosition = canvasInfo.offset;
            canvas.transform.localEulerAngles = new Vector3(0, 0, 0);
            canvas.transform.localScale = Vector3.one * 0.001f;
        }

        void FixMainMenuCanvas() {
            MoveCanvasToWorldSpace(new CanvasInfo("TitleCanvas"));
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

            public CanvasInfo(string _name, Vector3 _offset) {
                name = _name;
                offset = _offset;
            }
            public CanvasInfo(string _name) {
                name = _name;
                offset = new Vector3(0, 0, 1);
            }
        }
    }
}