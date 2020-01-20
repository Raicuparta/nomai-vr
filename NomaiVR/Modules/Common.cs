using OWML.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR
{
    public class Common : MonoBehaviour
    {
        public static PlayerCharacterController PlayerBody { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static Transform PlayerHead { get; private set; }
        public static ToolModeSwapper ToolSwapper { get; private set; }

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
            ToolSwapper = FindObjectOfType<ToolModeSwapper>();
        }

        public static List<GameObject> GetObjectsInLayer(GameObject root, int layer) {
            var ret = new List<GameObject>();
            var all = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject t in all) {
                if (t.layer == layer) {
                    ret.Add(t.gameObject);
                }
            }
            return ret;
        }
    }
}
