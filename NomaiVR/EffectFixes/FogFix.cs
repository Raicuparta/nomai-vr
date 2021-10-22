using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class FogFix : NomaiVRModule<FogFix.Behaviour, FogFix.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {
                FixDarkBrambleLights();
            }

            private static void FixDarkBrambleLights()
            {
                var fogLightCanvas = GameObject.Find("FogLightCanvas").GetComponent<Canvas>();
                fogLightCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                fogLightCanvas.worldCamera = Locator.GetActiveCamera().mainCamera;
                fogLightCanvas.planeDistance = 100;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<PlanetaryFogController>(nameof(PlanetaryFogController.ResetFogSettings), nameof(Patch.PatchResetFog));
                    Prefix<PlanetaryFogController>(nameof(PlanetaryFogController.UpdateFogSettings), nameof(Patch.PatchUpdateFog));
                    Prefix<FogOverrideVolume>(nameof(FogOverrideVolume.OverrideFogSettings), nameof(Patch.PatchOverrideFog));
                    Prefix<PlanetaryFogImageEffect>(nameof(PlanetaryFogImageEffect.OnRenderImage), nameof(Patch.PreFogImageEffectRenderImage));
                    Prefix<PlanetaryFogRenderer>(nameof(PlanetaryFogRenderer.CalcFrustumCorners), nameof(Patch.PreCalcFrustumCorners));
                    Prefix<HeightmapAmbientLightRenderer>(nameof(HeightmapAmbientLightRenderer.CalcFrustumCorners), nameof(Patch.Prefix_HeightmapAmbientLightRenderer_CalcFrustumCorners));
                }

                private static readonly Vector3[] frustumCornersBuffer = new Vector3[4];
                private static Matrix4x4 FrustumCornersMatrix(Camera cam, Camera.MonoOrStereoscopicEye eye)
                {
                    var camtr = cam.transform;
                    cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, eye, frustumCornersBuffer);

                    Matrix4x4 frustumMatrix = Matrix4x4.identity;
                    frustumMatrix.SetRow(0, camtr.TransformVector(frustumCornersBuffer[1])); //topLeft
                    frustumMatrix.SetRow(1, camtr.TransformVector(frustumCornersBuffer[2])); //topRight
                    frustumMatrix.SetRow(2, camtr.TransformVector(frustumCornersBuffer[3])); //bottomRight
                    frustumMatrix.SetRow(3, camtr.TransformVector(frustumCornersBuffer[0])); //bottomLeft
                    return frustumMatrix;
                }
                private static readonly int propIDRingworldFogClipPlane1 = Shader.PropertyToID("_RingworldFogClipPlane1");
                private static readonly int propIDRingworldFogClipPlane2 = Shader.PropertyToID("_RingworldFogClipPlane2");
                private static bool PreFogImageEffectRenderImage(RenderTexture source, RenderTexture destination, PlanetaryFogImageEffect __instance)
                {
                    if (__instance._camera == null)
                    {
                        __instance._camera = __instance.GetComponent<Camera>();
                    }
                    if (__instance.fogMaterial == null && __instance.fogShader != null)
                    {
                        __instance.fogMaterial = new Material(__instance.fogShader);
                    }
                    if (__instance._camera.stereoEnabled && __instance._camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
                        return false;
                    if (__instance.fogMaterial != null)
                    {
                        __instance.fogMaterial.SetMatrix("_FrustumCornersWS", FrustumCornersMatrix(__instance._camera, __instance._camera.stereoActiveEye));

                        PlanetaryFogController activeFogSphere = PlanetaryFogController.GetActiveFogSphere();
                        if (activeFogSphere != null && activeFogSphere.isRingworldFog)
                        {
                            Vector3 position = activeFogSphere.transform.position;
                            Vector3 up = activeFogSphere.transform.up;
                            Plane plane = new Plane(up, position - up * activeFogSphere.ringworldPlaneDist1);
                            Plane plane2 = new Plane(-up, position + up * activeFogSphere.ringworldPlaneDist2);
                            __instance.fogMaterial.EnableKeyword("USE_RINGWORLD_LIGHTING");
                            __instance.fogMaterial.SetVector(propIDRingworldFogClipPlane1, new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance));
                            __instance.fogMaterial.SetVector(propIDRingworldFogClipPlane2, new Vector4(plane2.normal.x, plane2.normal.y, plane2.normal.z, plane2.distance));
                        }
                        else
                        {
                            __instance.fogMaterial.DisableKeyword("USE_RINGWORLD_LIGHTING");
                        }

                        __instance.CustomGraphicsBlit(source, destination, __instance.fogMaterial);
                    }
                    return false;
                }

                private static bool Prefix_HeightmapAmbientLightRenderer_CalcFrustumCorners(HeightmapAmbientLightRenderer __instance, ref Matrix4x4 __result)
                {
                    __result = FrustumCornersMatrix(__instance._owCamera.mainCamera, __instance._owCamera.mainCamera.stereoActiveEye);
                    return false;
                }

                private static bool PreCalcFrustumCorners(PlanetaryFogRenderer __instance, ref Matrix4x4 __result)
                {
                    __result = FrustumCornersMatrix(__instance._owCamera.mainCamera, __instance._owCamera.mainCamera.stereoActiveEye);
                    return false;
                }

                private static bool PatchResetFog()
                {
                    return !Camera.current.stereoEnabled || Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
                }

                private static bool PatchUpdateFog()
                {
                    if (InputHelper.IsUIInteractionMode())
                    {
                        return false;
                    }
                    return !Camera.current.stereoEnabled || Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
                }

                private static bool PatchOverrideFog()
                {
                    if (InputHelper.IsUIInteractionMode())
                    {
                        return false;
                    }
                    return !Camera.current.stereoEnabled || Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
                }
            }
        }
    }
}
