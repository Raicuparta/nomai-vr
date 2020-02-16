using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR {
    public class Common: MonoBehaviour {
        public static PlayerCharacterController PlayerBody { get; private set; }
        public static Transform PlayerHead { get; private set; }
        public static ToolModeSwapper ToolSwapper { get; private set; }

        public static void InitGame () {
            PlayerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            PlayerHead = FindObjectOfType<ToolModeUI>().transform;
            PlayerHead.localPosition = new Vector3(PlayerHead.localPosition.x, PlayerHead.localPosition.y, 0);
            ToolSwapper = FindObjectOfType<ToolModeSwapper>();
        }

        public static List<GameObject> GetObjectsInLayer (GameObject root, int layer) {
            var ret = new List<GameObject>();
            var all = FindObjectsOfType<GameObject>();

            foreach (GameObject t in all) {
                if (t.layer == layer) {
                    ret.Add(t.gameObject);
                }
            }
            return ret;
        }
    }
}
