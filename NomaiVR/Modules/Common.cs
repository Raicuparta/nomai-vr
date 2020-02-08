using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR {
    public class Common: MonoBehaviour {
        public static PlayerCharacterController PlayerBody { get; private set; }
        public static Camera MainCamera { get; private set; }
        public static Transform PlayerHead { get; private set; }
        public static ToolModeSwapper ToolSwapper { get; private set; }

        void Awake () {
            NomaiVR.Log("Start Common");

            InitPreGame();
        }

        public static void InitPreGame () {
            MainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        public static void InitGame () {
            InitPreGame();
            PlayerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            PlayerHead = FindObjectOfType<ToolModeUI>().transform;
            PlayerHead.localPosition = new Vector3(PlayerHead.localPosition.x, PlayerHead.localPosition.y, 0);
            ToolSwapper = FindObjectOfType<ToolModeSwapper>();
        }

        public static List<GameObject> GetObjectsInLayer (GameObject root, int layer) {
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
