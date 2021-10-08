using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.XR;

namespace NomaiVR.EffectFixes
{
    internal class DreamFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, DreamFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Postfix<PostProcessingGameplaySettings>(nameof(PostProcessingGameplaySettings.ApplySettings), nameof(DisableScreenSpaceReflections));
                Prefix<MindProjectorImageEffect>(nameof(MindProjectorImageEffect.Awake), nameof(AddVRProjector));
                Prefix<MindProjectorImageEffect>(nameof(MindProjectorImageEffect.OnRenderImage), nameof(BlitImageEffect));
                Prefix<MindProjectorImageEffect>("set_eyeOpenness", nameof(SetEyeOpennes));
                Prefix<MindProjectorImageEffect>("set_slideTexture", nameof(SetSlideTexture));

                Prefix<LanternZoomPoint>(nameof(LanternZoomPoint.UpdateRetroZoom), nameof(UpdateRetrozoomFOVScale));
                Prefix<LanternZoomPoint>(nameof(LanternZoomPoint.UpdateZoomIn), nameof(UpdateZoomInFOVScale));
                Postfix<LanternZoomPoint>(nameof(LanternZoomPoint.FinishRetroZoom), nameof(ResetScaleFactor));
                Postfix<LanternZoomPoint>(nameof(LanternZoomPoint.StartRetroZoom), nameof(HeadIndependentHeading));
            }

            public static void DisableScreenSpaceReflections(PostProcessingGameplaySettings __instance)
            {
                __instance._runtimeProfile.screenSpaceReflection.enabled = false;
            }

            public static bool BlitImageEffect(MindProjectorImageEffect __instance, RenderTexture source, RenderTexture destination)
            {
                __instance.enabled = false;
                Graphics.Blit(source, destination);
                return false;
            }

            public static void AddVRProjector(MindProjectorImageEffect __instance)
            {
                var projector = __instance.gameObject.AddComponent<VRMindProjectorImageEffect>();
                projector.enabled = false;
            }

            public static void SetEyeOpennes(MindProjectorImageEffect __instance, float value)
            {
                var vrProjector = __instance.GetComponent<VRMindProjectorImageEffect>();
                if(!vrProjector.enabled) vrProjector.enabled = true;
                vrProjector.eyeOpenness = value;
            }

            public static void SetSlideTexture(MindProjectorImageEffect __instance, Texture value)
            {
                var vrProjector = __instance.GetComponent<VRMindProjectorImageEffect>();
                if (value == null) vrProjector.enabled = false;
            }

            public static bool UpdateZoomInFOVScale(LanternZoomPoint __instance)
            {
                float time = Mathf.InverseLerp(__instance._stateChangeTime, __instance._stateChangeTime + 0.5f, Time.time);
                float t = __instance._zoomInCurve.Evaluate(time);
                float targetFieldOfView = Mathf.Lerp(Locator.GetPlayerCameraController().GetOrigFieldOfView(), __instance._startFOV, t);
                XRDevice.fovZoomFactor = Locator.GetPlayerCamera().mainCamera.fieldOfView / targetFieldOfView;
                if (Time.time > __instance._stateChangeTime + 0.5f)
                {
                    __instance.ChangeState(LanternZoomPoint.State.RetroZoom);
                    __instance.StartRetroZoom();
                }
                return false;
            }

            public static bool UpdateRetrozoomFOVScale(LanternZoomPoint __instance)
            {
                float num = Mathf.InverseLerp(__instance._stateChangeTime, __instance._stateChangeTime + 1.2f, Time.time);
                float focus = Mathf.Pow(Mathf.SmoothStep(0f, 1f, 1f - num), 0.2f);
                __instance._playerLantern.GetLanternController().SetFocus(focus);
                float t = __instance._retroZoomCurve.Evaluate(num);
                float targetFieldOfView = Mathf.Lerp(__instance._startFOV, Locator.GetPlayerCameraController().GetOrigFieldOfView(), t);
                XRDevice.fovZoomFactor = Locator.GetPlayerCamera().mainCamera.fieldOfView / targetFieldOfView;
                float d = __instance._imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * 0.017453292f * 0.5f);
                Vector3 vector = __instance._startLocalPos - __instance._endLocalPos;
                __instance._attachPoint.transform.localPosition = __instance._endLocalPos + vector.normalized * d;
                if (num >= 1f)
                {
                    __instance.FinishRetroZoom();
                }

                return false;
            }

            public static void HeadIndependentHeading(LanternZoomPoint __instance)
            {
                var playerPos = Locator.GetPlayerTransform().position;
                var heading = (__instance.transform.position - playerPos).normalized;
                float d = __instance._imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * 0.017453292f * 0.5f);
                Vector3 position = playerPos + heading * d;
                __instance._endLocalPos = __instance.transform.InverseTransformPoint(position);
            }

            private static void ResetScaleFactor()
            {
                XRDevice.fovZoomFactor = 1;
            }
        }
    }
}
