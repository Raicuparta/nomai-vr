using UnityEngine;
using UnityEngine.SceneManagement;

namespace OWML.NomaiVR
{
    public class Menus : MonoBehaviour
    {
        protected class CanvasInfo {
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

        static readonly CanvasInfo[] _canvasInfos = {
            new CanvasInfo("PauseMenu", new Vector3(-0.6f, -0.5f, 1)),
            new CanvasInfo("DialogueCanvas"),
        };

        void Start() {
            NomaiVR.Log("Start Menus");

            SceneManager.sceneLoaded += OnSceneLoaded;
            MoveAllCanvasesToWorldSpace();
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            MovePauseMenuToWorldSpace();
        }

        void MoveAllCanvasesToWorldSpace() {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

            foreach (Canvas canvas in canvases) {
                canvas.renderMode = RenderMode.WorldSpace;
                Transform originalParent = canvas.transform.parent;
                canvas.transform.parent = Object.FindObjectOfType<Camera>().transform;
                canvas.transform.localPosition = new Vector3(0, 0, 1);
                canvas.transform.localRotation = new Quaternion(0, 0, 0, 1);
                canvas.transform.localScale = Vector3.one * 0.0005f;
            }
        }

        void MovePauseMenuToWorldSpace() {
            foreach (CanvasInfo canvasInfo in _canvasInfos) {
                GameObject canvas = GameObject.Find(canvasInfo.name);

                if (canvas == null) {
                    NomaiVR.Log("Couldn't find canvas with name: " + canvasInfo.name);
                    continue;
                }

                Canvas[] subCanvases = canvas.GetComponentsInChildren<Canvas>();
                Camera playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();

                foreach (Canvas subCanvas in subCanvases) {
                    subCanvas.renderMode = RenderMode.WorldSpace;
                }

                canvas.transform.parent = playerCamera.transform;
                canvas.transform.localPosition = canvasInfo.offset;
                canvas.transform.localEulerAngles = new Vector3(0, 0, 0);
                canvas.transform.localScale = Vector3.one * 0.001f;
            }
        }

    }
}