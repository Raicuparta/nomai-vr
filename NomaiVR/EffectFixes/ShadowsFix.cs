using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class ShadowsFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ShadowsFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                //Solve (1x)PreCull->(2x)PostRender conflicts
                Prefix<CSMTextureCacher>(nameof(CSMTextureCacher.OnAnyCameraPostRender), nameof(Patch.PreOnAnyCameraPostRender));
                Prefix<RingworldShadowsOverride>(nameof(RingworldShadowsOverride.OnCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<CloudEffectBubbleController>(nameof(CloudEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<FogWarpEffectBubbleController>(nameof(FogWarpEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<QuantumFogEffectBubbleController>(nameof(QuantumFogEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<SandEffectBubbleController>(nameof(SandEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<UnderwaterEffectBubbleController>(nameof(UnderwaterEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<PerCameraRendererState>(nameof(PerCameraRendererState.OnOWCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<GeyserWaterVillageHack>(nameof(GeyserWaterVillageHack.OnPlayerCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<LightLOD>(nameof(LightLOD.RevertLODSettings), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<RingworldSunController>(nameof(RingworldSunController.OnOWCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<IPExteriorVisualsManager>(nameof(IPExteriorVisualsManager.OnOWCameraPostRender), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<TessellatedSphereRenderer>(nameof(TessellatedSphereRenderer.Clear), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<TessellatedPlaneRenderer>(nameof(TessellatedPlaneRenderer.Clear), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<TessellatedRingRenderer>(nameof(TessellatedRingRenderer.Clear), nameof(Patch.OwCameraPostRenderDisabler));
                Prefix<UnderwaterCurrentFadeController>(nameof(UnderwaterCurrentFadeController.OnAnyPrerender), nameof(Patch.Pre_UnderwaterCurrentFadeController_OnAnyPrerender));
                Prefix<UnderwaterCurrentFadeController>(nameof(UnderwaterCurrentFadeController.OnAnyPostrender), nameof(Patch.Pre_UnderwaterCurrentFadeController_OnAnyPostRender));
            }

            private static bool OwCameraPostRenderDisabler(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            private static bool PreOnAnyCameraPostRender(Camera camera)
            {
                return !camera.stereoEnabled || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            //ProxyShadowLight updates for some reason have garbage data on the left eye, only update them on the right
            private static bool Pre_UnderwaterCurrentFadeController_OnAnyPrerender(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }

            private static bool Pre_UnderwaterCurrentFadeController_OnAnyPostRender(OWCamera owCamera)
            {
                return owCamera.mainCamera == null || !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }
        }
    }
}
