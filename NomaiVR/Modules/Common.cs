using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR
{
    public class Common : MonoBehaviour
    {
        public static PlayerCharacterController PlayerBody { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static Transform PlayerHead { get; private set; }

        void Awake() {
            NomaiVR.Log("Start Common");

            SceneManager.sceneLoaded += OnSceneLoaded;

            InitPreGame();
            NomaiVR.Helper.Events.Subscribe<Flashlight>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnWakeUp;
        }

        void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            InitGame();
        }
        private void OnWakeUp(MonoBehaviour behaviour, Events ev) { InitGame(); }

            void InitPreGame() {
            MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        void InitGame() {
            InitPreGame();
            PlayerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            PlayerHead = FindObjectOfType<ToolModeUI>().transform;
        }
    }
}
