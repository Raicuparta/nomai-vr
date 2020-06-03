using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    public class LookArrow : NomaiVRModule<LookArrow.Behaviour, LookArrow.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;
        private const float margin = 0.6f;

        public class Behaviour : MonoBehaviour
        {
            private Transform _rightArrow;
            private Transform _leftArrow;
            public static Transform Target;

            private void Start()
            {
                var canvas = Instantiate(AssetLoader.LookArrow).GetComponent<Canvas>();
                canvas.transform.parent = Locator.GetPlayerCamera().transform;
                canvas.transform.localPosition = new Vector3(0, 0, 4);
                canvas.transform.localRotation = Quaternion.identity;

                _rightArrow = canvas.transform.Find("look-right");
                _rightArrow.GetComponent<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();
                _rightArrow.gameObject.SetActive(false);
                _leftArrow = canvas.transform.Find("look-left");
                _leftArrow.GetComponent<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();
                _leftArrow.gameObject.SetActive(false);

            }

            private void Update()
            {
                if (Target == null)
                {
                    return;
                }
                var camera = Locator.GetPlayerCamera().transform;
                var targetDirection = (Target.position - camera.position).normalized;
                var perpendicular = Vector3.Cross(camera.forward, targetDirection);
                var dir = Vector3.Dot(perpendicular, camera.up);

                _rightArrow.gameObject.SetActive(dir > margin);
                _leftArrow.gameObject.SetActive(dir < -margin);
            }
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                var lockOnMethods = typeof(PlayerLockOnTargeting).GetMethods().Where(method => method.Name == "LockOn");
                foreach (var method in lockOnMethods)
                {
                    NomaiVR.Helper.HarmonyHelper.AddPrefix(method, typeof(Patch), nameof(PreLockOn));
                }
                var breakLockMethods = typeof(PlayerLockOnTargeting).GetMethods().Where(method => method.Name == "BreakLock");
                foreach (var method in breakLockMethods)
                {
                    NomaiVR.Helper.HarmonyHelper.AddPrefix(method, typeof(Patch), nameof(PreBreakLock));
                }
            }

            public static bool PreLockOn(Transform targetTransform)
            {
                if (targetTransform.GetComponent<ModelShipController>() == null)
                {
                    Behaviour.Target = targetTransform;
                }

                return false;
            }

            public static bool PreBreakLock()
            {
                Behaviour.Target = null;
                return false;
            }
        }
    }
}
