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

        public static List<GameObject> GetObjectsInLayer (string layerName) {
            var layer = LayerMask.NameToLayer(layerName);
            var ret = new List<GameObject>();
            var all = FindObjectsOfType<GameObject>();

            foreach (GameObject t in all) {
                if (t.layer == layer) {
                    ret.Add(t.gameObject);
                }
            }
            return ret;
        }

        public static bool IsUsingAnyTool () {
            return ToolSwapper.IsInToolMode(ToolMode.Probe) || ToolSwapper.IsInToolMode(ToolMode.Translator) || ToolSwapper.IsInToolMode(ToolMode.SignalScope);
        }

        public static void ChangeLayerRecursive (GameObject obj, string maskName) {
            ChangeLayerRecursive(obj, LayerMask.NameToLayer(maskName));
        }

        public static void ChangeLayerRecursive (GameObject obj, LayerMask mask) {
            obj.layer = mask;
            foreach (Transform child in obj.transform) {
                ChangeLayerRecursive(child.gameObject, mask);
            }
        }

        public static bool IsInGame () {
            return LoadManager.GetCurrentScene() == OWScene.SolarSystem || LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse;
        }
    }
}
