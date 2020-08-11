using UnityEngine;

namespace NomaiVR
{
    internal class DebugFollowTarget : MonoBehaviour
    {
        public float positionDelta = 0.1f;
        public float angleDelta = 10f;
        public Vector3 scaleDelta = Vector3.one * 0.0002f;

        private bool _angleMode;
        private FollowTarget _followTarget;

        internal void Awake()
        {
            _followTarget = gameObject.GetComponent<FollowTarget>();
        }

        internal void Update()
        {
            var position = _followTarget.localPosition;

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

            var rotation = _followTarget.localRotation;

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
                _followTarget.localPosition = position;
                _followTarget.localRotation = rotation;
                transform.localScale = scale;
                var angles = transform.localEulerAngles;

                Logs.Write("Position: new Vector3(" + Round(position.x) + "f, " + Round(position.y) + "f, " + Round(position.z) + "f)");
                Logs.Write("Rotation: Quaternion.Euler(" + Round(angles.x) + "f, " + Round(angles.y) + "f, " + Round(angles.z) + "f)");
                Logs.Write("Scale: " + scale);
            }
        }

        private static float Round(float value)
        {
            return Mathf.Round(value * 1000f) / 1000f;
        }
    }
}
