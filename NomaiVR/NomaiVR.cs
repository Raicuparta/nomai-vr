using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace OWML.NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        private void Start() {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();

            ModHelper.Console.WriteLine("found " + canvases.Length + " canvases.");
            foreach (Canvas canvas in canvases) {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.parent = Object.FindObjectOfType<Camera>().transform;
                canvas.transform.localPosition = new Vector3(-50, -30, 200);
                canvas.transform.localRotation = new Quaternion(0, 0, 0, 1);
                canvas.transform.localScale = Vector3.one * 0.2f;
                canvas.transform.parent = null;
            }
        }

        private void Update() {
        }

    }
}