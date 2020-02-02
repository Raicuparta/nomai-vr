using UnityEngine;

namespace NomaiVR {
    public class HoldPrompts: MonoBehaviour {
        Transform _holdTransform;
        int _debugChild = 0;
        Transform _canvasTransform;

        void Awake () {
            var canvas = GameObject.Find("ScreenPromptCanvas").GetComponent<Canvas>();
            _canvasTransform = canvas.transform;
            canvas.transform.localScale = Vector3.one * 0.0015f;
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localRotation = Quaternion.identity;

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localPosition = Vector3.zero;
            canvas.transform.localRotation = Quaternion.identity;

            _holdTransform = Hands.HoldObject(canvas.transform, Hands.RightHand, new Vector3(-0.09f, -0.11f, 0.13f));
            _holdTransform.gameObject.AddComponent<DebugTransform>();

            foreach (Transform child in canvas.transform) {
                child.localPosition = Vector3.zero;
            }
        }

        void Update () {
            _holdTransform.LookAt(2 * _holdTransform.position - Common.MainCamera.transform.position, Common.PlayerHead.up);

            if (Input.GetKeyDown(KeyCode.Keypad3)) {
                Destroy(_canvasTransform.GetChild(_debugChild).gameObject.GetComponent<DebugTransform>());
                if (_debugChild < 5)
                    _debugChild += 1;
                else {
                    _debugChild = 0;
                }
                _canvasTransform.GetChild(_debugChild).gameObject.AddComponent<DebugTransform>();
            }
        }
    }
}