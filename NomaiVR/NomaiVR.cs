using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OWML.NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        void Start() {
            SceneManager.sceneLoaded += OnSceneLoaded;
            MoveMainMenuToWorldSpace();
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            MovePauseMenuToWorldSpace();
        }

        void MoveMainMenuToWorldSpace() {
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
            GameObject pauseMenu = GameObject.Find("PauseMenu");
            Canvas[] canvases = pauseMenu.GetComponentsInChildren<Canvas>();
            Camera playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();

            foreach (Canvas canvas in canvases) {
                canvas.renderMode = RenderMode.WorldSpace;
            }

            pauseMenu.transform.parent = playerCamera.transform;
            pauseMenu.transform.localPosition = new Vector3(-0.6f, -0.5f, 1);
            pauseMenu.transform.localEulerAngles = new Vector3(0, 0, 0);
            pauseMenu.transform.localScale = Vector3.one * 0.001f;
        }

    }
}