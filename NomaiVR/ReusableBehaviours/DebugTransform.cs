using UnityEngine;

namespace NomaiVR
{
    internal class DebugTransform : MonoBehaviour
    {
        private bool _angleMode;
        public float positionDelta = 0.02f;
        public float angleDelta = 10f;
        public Vector3 scaleDelta = Vector3.one * 0.02f;

        private static float Round(float value)
        {
            return Mathf.Round(value * 1000f) / 1000f;
        }

        internal void Update()
        {
            var position = transform.localPosition;

            if (!_angleMode)
            {
                if (Input.GetKeyDown(KeyCode.Keypad7))
                {
                    position.x += positionDelta;
                }
                if (Input.GetKeyDown(KeyCode.Keypad4))
                {
                    position.x -= positionDelta;
                }
                if (Input.GetKeyDown(KeyCode.Keypad8))
                {
                    position.y += positionDelta;
                }
                if (Input.GetKeyDown(KeyCode.Keypad5))
                {
                    position.y -= positionDelta;
                }
                if (Input.GetKeyDown(KeyCode.Keypad9))
                {
                    position.z += positionDelta;
                }
                if (Input.GetKeyDown(KeyCode.Keypad6))
                {
                    position.z -= positionDelta;
                }
            }

            var rotation = transform.localRotation;

            if (_angleMode)
            {
                if (Input.GetKeyDown(KeyCode.Keypad7))
                {
                    rotation *= Quaternion.Euler(angleDelta, 0, 0);
                }
                if (Input.GetKeyDown(KeyCode.Keypad4))
                {
                    rotation *= Quaternion.Euler(-angleDelta, 0, 0);
                }
                if (Input.GetKeyDown(KeyCode.Keypad8))
                {
                    rotation *= Quaternion.Euler(0, angleDelta, 0);
                }
                if (Input.GetKeyDown(KeyCode.Keypad5))
                {
                    rotation *= Quaternion.Euler(0, -angleDelta, 0);
                }
                if (Input.GetKeyDown(KeyCode.Keypad9))
                {
                    rotation *= Quaternion.Euler(0, 0, angleDelta);
                }
                if (Input.GetKeyDown(KeyCode.Keypad6))
                {
                    rotation *= Quaternion.Euler(0, 0, -angleDelta);
                }
            }

            var scale = transform.localScale;

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                scale += scaleDelta;
            }
            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                scale -= scaleDelta;
            }

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                _angleMode = !_angleMode;
            }

            if (Input.anyKeyDown)
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
                transform.localScale = scale;
                var angles = transform.localEulerAngles;

                Logs.Write("Position: new Vector3(" + Round(position.x) + "f, " + Round(position.y) + "f, " + Round(position.z) + "f)");
                Logs.Write("Rotation: Quaternion.Euler(" + Round(angles.x) + "f, " + Round(angles.y) + "f, " + Round(angles.z) + "f)");
                Logs.Write("Scale: " + scale);
            }
        }
    }
}
