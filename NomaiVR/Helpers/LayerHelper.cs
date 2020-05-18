using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR
{
    public static class LayerHelper
    {
        public static List<GameObject> GetObjectsInLayer(string layerName)
        {
            var layer = LayerMask.NameToLayer(layerName);
            var ret = new List<GameObject>();
            var all = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject t in all)
            {
                if (t.layer == layer)
                {
                    ret.Add(t.gameObject);
                }
            }
            return ret;
        }

        public static void ChangeLayerRecursive(GameObject obj, string maskName)
        {
            ChangeLayerRecursive(obj, LayerMask.NameToLayer(maskName));
        }

        public static void ChangeLayerRecursive(GameObject obj, LayerMask mask)
        {
            obj.layer = mask;
            foreach (Transform child in obj.transform)
            {
                ChangeLayerRecursive(child.gameObject, mask);
            }
        }
    }
}
