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
            public static bool IsLockedOn = false;

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

            private void Update()
            {
                _rightArrow.gameObject.SetActive(IsLockedOn);
                _leftArrow.gameObject.SetActive(IsLockedOn);
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

            public static bool PreLockOn()
            {
                NomaiVR.Log("Locked On!!");
                Behaviour.IsLockedOn = true;
                return false;
            }

            public static bool PreBreakLock()
            {
                NomaiVR.Log("Locked Off!!");
                Behaviour.IsLockedOn = false;
                return false;
            }
        }
    }
}
