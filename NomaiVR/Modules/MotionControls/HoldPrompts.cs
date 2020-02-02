using UnityEngine;

namespace NomaiVR {
    public class HoldPrompts: MonoBehaviour {
        Transform _holdTransform;

        void Awake () {
            var canvas = GameObject.Find("ScreenPromptCanvas").GetComponent<Canvas>();
            canvas.transform.localScale = Vector3.one * 0.0015f;
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localRotation = Quaternion.identity;

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localRotation = Quaternion.identity;

            _holdTransform = Hands.HoldObject(canvas.transform, Hands.RightHand, new Vector3(0.21f, 0f, 0.12f));
        }

        void Update () {
            _holdTransform.LookAt(Common.MainCamera.transform.position + Common.MainCamera.transform.forward * 30, Common.PlayerHead.up);
        }
    }
}