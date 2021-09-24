using UnityEngine;

namespace NomaiVR
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
                    Prefix<PlanetaryFogController>("ResetFogSettings", nameof(Patch.PatchResetFog));
                    Prefix<PlanetaryFogController>("UpdateFogSettings", nameof(Patch.PatchUpdateFog));
                    Prefix<FogOverrideVolume>("OverrideFogSettings", nameof(Patch.PatchOverrideFog));
                    Prefix<PlanetaryFogImageEffect>("OnRenderImage", nameof(Patch.PreFogImageEffectRenderImage));
                    Prefix<PlanetaryFogRenderer>("CalcFrustumCorners", nameof(Patch.PreCalcFrustumCorners));
                }

                private static Vector3[] _frustumCornersBuffer = new Vector3[4];
                private static Matrix4x4 FrustumCornersMatrix(Camera cam, Camera.MonoOrStereoscopicEye eye)
                {
                    var camtr = cam.transform;
                    cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, eye, _frustumCornersBuffer);

                    Matrix4x4 frustumMatrix = Matrix4x4.identity;
                    frustumMatrix.SetRow(0, camtr.TransformVector(_frustumCornersBuffer[1])); //topLeft
                    frustumMatrix.SetRow(1, camtr.TransformVector(_frustumCornersBuffer[2])); //topRight
                    frustumMatrix.SetRow(2, camtr.TransformVector(_frustumCornersBuffer[3])); //bottomRight
                    frustumMatrix.SetRow(3, camtr.TransformVector(_frustumCornersBuffer[0])); //bottomLeft
                    return frustumMatrix;
                }

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
                        __instance.CustomGraphicsBlit(source, destination, __instance.fogMaterial);
                    }
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
