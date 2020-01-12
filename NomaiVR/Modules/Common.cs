using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR
{
    public class Common : MonoBehaviour
    {
        public static PlayerCharacterController PlayerBody;
        public static Camera MainCamera;
        public static Transform PlayerHead;
        public static bool IsAwake;

        void Start() {
            NomaiVR.Log("Start Common");

            SceneManager.sceneLoaded += OnSceneLoaded;

            InitPreGame();
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            InitGame();
        }

        void InitPreGame() {
            MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        void InitGame() {
            InitPreGame();
            IsAwake = true;
            PlayerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            PlayerHead = FindObjectOfType<ToolModeUI>().transform;
        }
    }
}
