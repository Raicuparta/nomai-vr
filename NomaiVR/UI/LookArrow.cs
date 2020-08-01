using System.Linq;
using UnityEngine;

namespace NomaiVR
{
    internal class LookArrow : NomaiVRModule<LookArrow.Behaviour, LookArrow.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform _rightArrow;
            private Transform _leftArrow;
            private Transform _wrapper;

            private static Transform _target;
            private static bool _pauseNextFrame;
            private InputMode? _prevInputMode;

            internal void Start()
            {
                var canvas = Instantiate(AssetLoader.LookArrowPrefab).GetComponent<Canvas>();
                _wrapper = canvas.transform;
                _wrapper.parent = Locator.GetPlayerCamera().transform;
                _wrapper.localPosition = new Vector3(0, 0, 4);
                _wrapper.localRotation = Quaternion.identity;

                _rightArrow = canvas.transform.Find("look-right");
                _rightArrow.GetComponent<SpriteRenderer>().material = MaterialHelper.GetOverlayMaterial();
                _rightArrow.gameObject.SetActive(false);
                _leftArrow = canvas.transform.Find("look-left");
                _leftArrow.GetComponent<SpriteRenderer>().material = MaterialHelper.GetOverlayMaterial();
                _leftArrow.gameObject.SetActive(false);
            }

            internal void Update()
            {
                UpdatePause();
                UpdateArrow();
            }

            private void DisableInput()
            {
                if (_prevInputMode != null)
                {
                    return;
                }
                _prevInputMode = OWInput.GetInputMode();
                OWInput.ChangeInputMode(InputMode.None);
                NomaiVR.Log("Disabling input from", _prevInputMode, "to", InputMode.None);
            }

            private void ResetInput()
            {
                if (_prevInputMode == null)
                {
                    return;
                }
                OWInput.ChangeInputMode((InputMode)_prevInputMode);
                NomaiVR.Log("Resetting input to", _prevInputMode);
                _prevInputMode = null;
            }

            private void ShowArrow()
            {
                DisableInput();
                _wrapper.gameObject.SetActive(true);
            }

            private void HideArrow()
            {
                ResetInput();
                _wrapper.gameObject.SetActive(false);
            }

            private void UpdatePause()
            {
                if (_pauseNextFrame)
                {
                    Time.timeScale = 0;
                    _pauseNextFrame = false;
                }
            }

            private void UpdateArrow()
            {
                if (_target == null)
                {
                    HideArrow();
                    return;
                }
                var screenPoint = Locator.GetPlayerCamera().mainCamera.WorldToViewportPoint(_target.position);
                var onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                if (onScreen)
                {
                    HideArrow();
                    return;
                }
                ShowArrow();

                var camera = Locator.GetPlayerCamera().transform;
                var targetDirection = (_target.position - camera.position).normalized;
                var perpendicular = Vector3.Cross(camera.forward, targetDirection);
                var player = Locator.GetPlayerTransform();
                var dir = Vector3.Dot(perpendicular, player.up);

                var forwardDot = Vector3.Dot(camera.forward, targetDirection);
                var isInFront = forwardDot > -0.5f;

                _rightArrow.gameObject.SetActive(dir > 0);
                _leftArrow.gameObject.SetActive(dir < 0);

                var playerHead = PlayerHelper.PlayerHead;
                if (isInFront)
                {
                    _wrapper.up = player.up;
                    _wrapper.LookAt(playerHead.position, targetDirection);
                    _wrapper.Rotate(new Vector3(0, 180, dir > 0 ? 90 : -90));
                }
                else
                {
                    _wrapper.up = player.up;
                    _wrapper.LookAt(2 * _wrapper.position - playerHead.position, playerHead.up);
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    var lockOnMethods = typeof(PlayerLockOnTargeting).GetMethods().Where(method => method.Name == "LockOn");
                    foreach (var method in lockOnMethods)
                    {
                        Prefix(method, nameof(PreLockOn));
                    }
                    var breakLockMethods = typeof(PlayerLockOnTargeting).GetMethods().Where(method => method.Name == "BreakLock");
                    foreach (var method in breakLockMethods)
                    {
                        Prefix(method, nameof(PreBreakLock));
                    }
                    Prefix(typeof(OWTime).GetMethod("Pause"), nameof(PrePause));
                }

                public static bool PrePause(OWTime.PauseType pauseType)
                {
                    if (pauseType == OWTime.PauseType.Reading)
                    {
                        _pauseNextFrame = true;
                        return false;
                    }
                    return true;
                }

                public static bool PreLockOn(Transform targetTransform)
                {
                    if (targetTransform.GetComponent<ModelShipController>() == null)
                    {
                        _target = targetTransform;
                    }

                    return false;
                }

                public static bool PreBreakLock()
                {
                    _target = null;
                    return false;
                }
            }
        }
    }
}
