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

        public class Behaviour : MonoBehaviour
        {
            private Transform _rightArrow;
            private Transform _leftArrow;
            public static Transform Target;

            private void Start()
            {
                var canvas = Instantiate(AssetLoader.LookArrow).GetComponent<Canvas>();
                //canvas.worldCamera = Locator.GetPlayerCamera().mainCamera;
                //canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.parent = Locator.GetPlayerCamera().transform;
                canvas.transform.localPosition = new Vector3(0, 0, 4);
                canvas.transform.localRotation = Quaternion.identity;

                _rightArrow = canvas.transform.Find("look-right");
                _rightArrow.GetComponent<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();
                _leftArrow = canvas.transform.Find("look-left");
                _leftArrow.GetComponent<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();

            }

            float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
            {
                Vector3 perp = Vector3.Cross(fwd, targetDir);
                float dir = Vector3.Dot(perp, up);

                if (dir > 0f)
                {
                    return 1f;
                }
                else if (dir < 0f)
                {
                    return -1f;
                }
                else
                {
                    return 0f;
                }
            }

            private void Update()
            {
                if (Target == null)
                {
                    return;
                }
                var camera = Locator.GetPlayerCamera().transform;
                var targetDirection = Target.position - camera.position;
                var dir = AngleDir(camera.forward, targetDirection, camera.up);

                _rightArrow.gameObject.SetActive(dir > 0);
                _leftArrow.gameObject.SetActive(dir < 0);
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
                NomaiVR.Log("Locked On!!");
                Behaviour.Target = targetTransform;
                return false;
            }

            public static bool PreBreakLock()
            {
                NomaiVR.Log("Locked Off!!");
                Behaviour.Target = null;
                return false;
            }
        }
    }
}
