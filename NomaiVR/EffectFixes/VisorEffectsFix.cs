
using System.Collections;
using UnityEngine;

namespace NomaiVR
{
    internal class VisorEffectsFix : NomaiVRModule<VisorEffectsFix.Behaviour, VisorEffectsFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;
        public const float WATER_EXIT_COOLDOWN = 8.0f;
        public static bool s_canShowCameraWaterEffect = true;

        public class Behaviour : MonoBehaviour
        {
            private IEnumerator _cameraWaterExitEffectCooldown;

            internal void Start()
            {
                // Disable water entering and exiting effect.
                var visorEffects = FindObjectOfType<VisorEffectController>();
                visorEffects._waterClearLength = 1f;
                visorEffects._waterFadeInLength = 0.2f;

                //Move Visor Effect renderers further to avoid clipping
                visorEffects._crackEffectRenderer.transform.localPosition += Vector3.down * 0.07f;
                visorEffects._visorEffectRenderer.transform.localPosition += Vector3.down * 0.1f;
                visorEffects._rainDropletsParticleSystem.transform.localPosition += Vector3.down * 0.09f;
                visorEffects._rainStreaksParticleSystem.transform.localPosition += Vector3.down * 0.09f;

                //Enable cooldown on water exit effect
                GlobalMessenger.AddListener("PlayerCameraExitWater", OnCameraExitWater);
            }

            internal void OnDestroy()
            {
                StopAllCoroutines();
                GlobalMessenger.RemoveListener("PlayerCameraExitWater", OnCameraExitWater);
            }

            internal void OnCameraExitWater()
            {
                if (_cameraWaterExitEffectCooldown != null)
                    StopCoroutine(_cameraWaterExitEffectCooldown);
                StartCoroutine(_cameraWaterExitEffectCooldown = WaitCameraWaterEffectCooldown());
            }

            internal IEnumerator WaitCameraWaterEffectCooldown()
            {
                yield return new WaitForSeconds(WATER_EXIT_COOLDOWN);
                _cameraWaterExitEffectCooldown = null;
                s_canShowCameraWaterEffect = true;
            }
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Postfix<VisorEffectController>(nameof(VisorEffectController.OnCameraExitWater), nameof(PostOnCameraExitWater));
                Postfix<UnderwaterEffectBubbleController>(nameof(UnderwaterEffectBubbleController.LateUpdate), nameof(DisableUnderwaterDistorsion));
            }

            public static void PostOnCameraExitWater(VisorEffectController __instance)
            {
                if (!s_canShowCameraWaterEffect)
                    __instance._waterClearTimer = __instance._waterClearLength;
                s_canShowCameraWaterEffect = false;
            }

            public static void DisableUnderwaterDistorsion(UnderwaterEffectBubbleController __instance)
            {
                if (__instance._fluidDetector != null)
                {
                    __instance._matPropBlock.SetFloat(__instance._propID_DistortMag, 0);
                    __instance._effectBubbleRenderer.SetPropertyBlock(__instance._matPropBlock);
                }
            }
        }
    }
}
