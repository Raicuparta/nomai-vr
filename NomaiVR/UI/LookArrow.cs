using System.Linq;
using NomaiVR.Assets;
using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.UI
{
    internal class LookArrow : NomaiVRModule<LookArrow.Behaviour, LookArrow.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform rightArrow;
            private Transform leftArrow;
            private Transform wrapper;

            private static Transform target;
            private static bool pauseNextFrame;

            internal void Start()
            {
                var canvas = Instantiate(AssetLoader.LookArrowPrefab).GetComponent<Canvas>();
                wrapper = canvas.transform;
                wrapper.parent = Locator.GetPlayerCamera().transform;
                wrapper.localPosition = new Vector3(0, 0, 4);
                wrapper.localRotation = Quaternion.identity;

                rightArrow = canvas.transform.Find("look-right");
                rightArrow.GetComponent<SpriteRenderer>().material = MaterialHelper.GetOverlayMaterial();
                rightArrow.gameObject.SetActive(false);
                leftArrow = canvas.transform.Find("look-left");
                leftArrow.GetComponent<SpriteRenderer>().material = MaterialHelper.GetOverlayMaterial();
                leftArrow.gameObject.SetActive(false);
            }

            internal void Update()
            {
                UpdatePause();
                UpdateArrow();
            }

            private void ShowArrow()
            {
                // ControllerInput.IsInputEnabled = false;
                wrapper.gameObject.SetActive(true);
            }

            private void HideArrow()
            {
                // ControllerInput.IsInputEnabled = true;
                wrapper.gameObject.SetActive(false);
            }

            private void UpdatePause()
            {
                if (pauseNextFrame)
                {
                    Time.timeScale = 0;
                    pauseNextFrame = false;
                }
            }

            private void UpdateArrow()
            {
                if (target == null)
                {
                    HideArrow();
                    return;
                }
                if (CameraHelper.IsOnScreen(target.position))
                {
                    HideArrow();
                    return;
                }
                ShowArrow();

                var camera = Locator.GetPlayerCamera().transform;
                var targetDirection = (target.position - camera.position).normalized;
                var perpendicular = Vector3.Cross(camera.forward, targetDirection);
                var player = Locator.GetPlayerTransform();
                var dir = Vector3.Dot(perpendicular, player.up);

                var forwardDot = Vector3.Dot(camera.forward, targetDirection);
                var isInFront = forwardDot > -0.5f;

                rightArrow.gameObject.SetActive(dir > 0);
                leftArrow.gameObject.SetActive(dir < 0);

                var playerHead = PlayerHelper.PlayerHead;
                if (isInFront)
                {
                    wrapper.up = player.up;
                    wrapper.LookAt(playerHead.position, targetDirection);
                    wrapper.Rotate(new Vector3(0, 180, dir > 0 ? 90 : -90));
                }
                else
                {
                    wrapper.up = player.up;
                    wrapper.LookAt(2 * wrapper.position - playerHead.position, playerHead.up);
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    var lockOnMethods = typeof(PlayerLockOnTargeting).GetMethods().Where(method => method.Name == nameof(PlayerLockOnTargeting.LockOn));
                    foreach (var method in lockOnMethods)
                    {
                        Prefix(method, nameof(PreLockOn));
                    }
                    var breakLockMethods = typeof(PlayerLockOnTargeting).GetMethods().Where(method => method.Name == nameof(PlayerLockOnTargeting.BreakLock));
                    foreach (var method in breakLockMethods)
                    {
                        Prefix(method, nameof(PreBreakLock));
                    }
                    Postfix(typeof(OWTime).GetMethod(nameof(OWTime.Pause)), nameof(PostPause));
                }

                public static void PostPause(OWTime.PauseType pauseType)
                {
                    if (pauseType == OWTime.PauseType.Reading)
                    {
                        Time.timeScale = 1;
                        pauseNextFrame = true;
                    }
                }

                public static bool PreLockOn(Transform targetTransform)
                {
                    if (targetTransform.GetComponent<ModelShipController>() == null && targetTransform.name != "NomaiHeadStatue")
                    {
                        target = targetTransform;
                    }
                    return false;
                }

                public static bool PreBreakLock()
                {
                    target = null;
                    return false;
                }
            }
        }
    }
}
