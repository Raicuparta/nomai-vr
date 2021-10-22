using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using NomaiVR.ReusableBehaviours.Dream;
using UnityEngine;
using UnityEngine.XR;

namespace NomaiVR.EffectFixes
{
    internal class DreamFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, DreamFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;
        private static float prePauseFovFactor = 1;
        private static bool isPaused = false;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                //Post Processing
                Postfix<PostProcessingGameplaySettings>(nameof(PostProcessingGameplaySettings.ApplySettings), nameof(DisableScreenSpaceReflections));

                //Mind Projectors
                Prefix<MindProjectorImageEffect>(nameof(MindProjectorImageEffect.Awake), nameof(AddVRProjector));
                Prefix<MindProjectorImageEffect>(nameof(MindProjectorImageEffect.OnRenderImage), nameof(BlitImageEffect));
                Prefix<MindProjectorImageEffect>("set_eyeOpenness", nameof(SetEyeOpennes));
                Prefix<MindProjectorImageEffect>("set_slideTexture", nameof(SetSlideTexture));

                //Zoom Points
                Prefix<LanternZoomPoint>(nameof(LanternZoomPoint.UpdateRetroZoom), nameof(UpdateRetrozoomFOVScale));
                Prefix<LanternZoomPoint>(nameof(LanternZoomPoint.UpdateZoomIn), nameof(UpdateZoomInFOVScale));
                Postfix<LanternZoomPoint>(nameof(LanternZoomPoint.FinishRetroZoom), nameof(ResetScaleFactor));
                Postfix<LanternZoomPoint>(nameof(LanternZoomPoint.StartRetroZoom), nameof(HeadIndependentHeading));

                //Simulation Camera
                Postfix<SimulationCamera>(nameof(SimulationCamera.Awake), nameof(Post_SimulationCamera_Awake));
                Postfix<SimulationCamera>(nameof(SimulationCamera.OnPreRender), nameof(Post_SimulationCamera_OnPreRender));
                Postfix<SimulationCamera>(nameof(SimulationCamera.OnEnable), nameof(Post_SimulationCamera_OnEnable));
                Postfix<SimulationCamera>(nameof(SimulationCamera.OnDisable), nameof(Post_SimulationCamera_OnDisable));
                Postfix<SimulationCamera>(nameof(SimulationCamera.DeallocateRenderTex), nameof(Post_SimulationCamera_DeallocateRenderTex));
                Prefix<SimulationCamera>(nameof(SimulationCamera.AllocateRenderTex), nameof(Pre_SimulationCamera_AllocateRenderTex));
                Prefix<SimulationCamera>(nameof(SimulationCamera.VerifyRenderTexResolution), nameof(Pre_SimulationCamera_VerifyRenderTexResolution));
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
                vrProjector.EyeOpenness = value;
            }

            public static void SetSlideTexture(MindProjectorImageEffect __instance, Texture value)
            {
                var vrProjector = __instance.GetComponent<VRMindProjectorImageEffect>();
                if (value == null) vrProjector.enabled = false;
            }

            public static bool UpdateZoomInFOVScale(LanternZoomPoint __instance)
            {
                if (!UpdatePauseFov()) return false;
                float time = Mathf.InverseLerp(__instance._stateChangeTime, __instance._stateChangeTime + 0.5f, Time.time);
                float t = __instance._zoomInCurve.Evaluate(time);
                float targetFieldOfView = Mathf.Lerp(Locator.GetPlayerCameraController().GetOrigFieldOfView(), __instance._startFOV, t);
                CameraHelper.SetFieldOfViewFactor(Locator.GetPlayerCamera().mainCamera.fieldOfView / targetFieldOfView);
                if (Time.time > __instance._stateChangeTime + 0.5f)
                {
                    __instance.ChangeState(LanternZoomPoint.State.RetroZoom);
                    __instance.StartRetroZoom();
                }
                return false;
            }

            public static bool UpdateRetrozoomFOVScale(LanternZoomPoint __instance)
            {
                if (!UpdatePauseFov()) return false;
                float num = Mathf.InverseLerp(__instance._stateChangeTime, __instance._stateChangeTime + 1.2f, Time.time);
                float focus = Mathf.Pow(Mathf.SmoothStep(0f, 1f, 1f - num), 0.2f);
                __instance._playerLantern.GetLanternController().SetFocus(focus);
                float t = __instance._retroZoomCurve.Evaluate(num);
                float targetFieldOfView = Mathf.Lerp(__instance._startFOV, Locator.GetPlayerCameraController().GetOrigFieldOfView(), t);
                CameraHelper.SetFieldOfViewFactor(Locator.GetPlayerCamera().mainCamera.fieldOfView / targetFieldOfView);
                float d = __instance._imageHalfWidth / Mathf.Tan(Locator.GetPlayerCamera().fieldOfView * 0.017453292f * 0.5f);
                Vector3 vector = __instance._startLocalPos - __instance._endLocalPos;
                __instance._attachPoint.transform.localPosition = __instance._endLocalPos + vector.normalized * d;
                if (num >= 1f)
                {
                    __instance.FinishRetroZoom();
                }

                return false;
            }

            private static bool UpdatePauseFov()
            {
                if (InputHelper.IsUIInteractionMode())
                {
                    if(!isPaused)
                    {
                        isPaused = true;
                        prePauseFovFactor = CameraHelper.GetFieldOfViewFactor();
                        ResetScaleFactor();
                    }
                    return false;
                }

                if (!InputHelper.IsUIInteractionMode() && isPaused)
                {
                    CameraHelper.SetFieldOfViewFactor(prePauseFovFactor);
                    isPaused = false;
                }
                return true;
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
                CameraHelper.SetFieldOfViewFactor(1, true);
            }

            private static void Post_SimulationCamera_Awake(SimulationCamera __instance)
            {
                __instance._camera.stereoTargetEye = StereoTargetEyeMask.Left;
                __instance._camera.cullingMask = LayerMask.GetMask("DreamSimulation", "UI");
                var supportCamera = new GameObject("StereoSupportCamera");
                supportCamera.transform.SetParent(__instance.transform, false);
                supportCamera.transform.localPosition = Vector3.zero;
                supportCamera.transform.localRotation = Quaternion.identity;
                var simSupportCam = supportCamera.AddComponent<SupportSimulationCamera>();
                simSupportCam.SetupSimulationCameraParent(__instance);
            }

            private static void Post_SimulationCamera_OnEnable(SimulationCamera __instance)
            {
                if (__instance._targetCamera != null && __instance._targetCamera.mainCamera.stereoEnabled)
                {
                    __instance.GetComponentInChildren<SupportSimulationCamera>().enabled = true;
                }

                GlobalMessenger.FireEvent("SimulationEnter");
            }

            private static void Post_SimulationCamera_OnDisable(SimulationCamera __instance)
            {
                if (__instance._targetCamera != null && __instance._targetCamera.mainCamera.stereoEnabled)
                {
                    __instance.GetComponentInChildren<SupportSimulationCamera>().enabled = false;
                }

                GlobalMessenger.FireEvent("SimulationExit");
            }

            private static void Post_SimulationCamera_OnPreRender(SimulationCamera __instance)
            {
                if (__instance._targetCamera == null)
                {
                    return;
                }
                GraphicsHelper.ForceCameraToEye(__instance._camera, __instance._targetCamera.mainCamera, Valve.VR.EVREye.Eye_Left);
            }

            private static void Pre_SimulationCamera_VerifyRenderTexResolution(SimulationCamera __instance)
            {
                __instance.GetComponentInChildren<SupportSimulationCamera>().VerifyRenderTexResolution(__instance._targetCamera.mainCamera);
            }

            private static void Pre_SimulationCamera_AllocateRenderTex(SimulationCamera __instance)
            {
                __instance.GetComponentInChildren<SupportSimulationCamera>().AllocateTexture();
            }

            private static void Post_SimulationCamera_DeallocateRenderTex(SimulationCamera __instance)
            {
                __instance.GetComponentInChildren<SupportSimulationCamera>().DeallocateTexture();
            }
        }
    }
}
