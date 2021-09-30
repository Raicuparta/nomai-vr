
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
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
                Prefix<RingworldShadowsOverride>(nameof(RingworldShadowsOverride.OnCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<CloudEffectBubbleController>(nameof(CloudEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<FogWarpEffectBubbleController>(nameof(FogWarpEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<QuantumFogEffectBubbleController>(nameof(QuantumFogEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<SandEffectBubbleController>(nameof(SandEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //Prefix<UnderwaterEffectBubbleController>(nameof(UnderwaterEffectBubbleController.OnTargetCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                //FIXME: These seem to break even more ?_?
                Prefix<PerCameraRendererState>(nameof(PerCameraRendererState.OnOWCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<GeyserWaterVillageHack>(nameof(GeyserWaterVillageHack.OnPlayerCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<LightLOD>(nameof(LightLOD.RevertLODSettings), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<RingworldSunController>(nameof(RingworldSunController.OnOWCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<IPExteriorVisualsManager>(nameof(IPExteriorVisualsManager.OnOWCameraPostRender), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<TessellatedSphereRenderer>(nameof(TessellatedSphereRenderer.Clear), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<TessellatedPlaneRenderer>(nameof(TessellatedPlaneRenderer.Clear), nameof(Patch.OWCameraPostRenderDisabler));
                Prefix<TessellatedRingRenderer>(nameof(TessellatedRingRenderer.Clear), nameof(Patch.OWCameraPostRenderDisabler));

                //Ensure Light Event order
                //FIXME: even after this, lighting is still wrong
                Prefix<ProxyShadowLight>(nameof(ProxyShadowLight.OnEnable), nameof(Patch.Pre_ProxyShadowLight_Enable));
                Prefix<ProxyShadowLight>(nameof(ProxyShadowLight.OnDisable), nameof(Patch.Pre_ProxyShadowLight_Disable));
                Prefix<CubeLight>(nameof(CubeLight.AddCommandBuffers), nameof(Patch.Pre_CubeLight_AddCommandBuffers));
                Prefix<CubeLight>(nameof(CubeLight.RemoveCommandBuffers), nameof(Patch.Pre_CubeLight_RemoveCommandBuffers));
            }

            private static bool OWCameraPostRenderDisabler(OWCamera owCamera)
            {
                return !owCamera.mainCamera.stereoEnabled || owCamera.mainCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            private static bool PreOnAnyCameraPostRender(Camera camera)
            {
                return !camera.stereoEnabled || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }

            private static bool Pre_ProxyShadowLight_Enable(ProxyShadowLight __instance)
            {
                __instance._light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, __instance._renderCommandBuffer);
                __instance._light.AddCommandBuffer(LightEvent.AfterShadowMap, __instance._paramsCommandBuffer);
                Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(__instance.BuildCommandBuffers));
                Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(__instance.ClearCommandBuffers));
                return false;
            }

            private static bool Pre_ProxyShadowLight_Disable(ProxyShadowLight __instance)
            {
                __instance._light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, __instance._renderCommandBuffer);
                __instance._light.RemoveCommandBuffer(LightEvent.AfterShadowMap, __instance._paramsCommandBuffer);
                Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(__instance.BuildCommandBuffers));
                Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(__instance.ClearCommandBuffers));
                return false;
            }

            //FIXME: The order here is probably scuffed, I didn't see a single light of this kind for now... probably DLC content
            private static bool Pre_CubeLight_AddCommandBuffers(CubeLight __instance)
            {
                __instance._light.AddCommandBuffer(LightEvent.AfterScreenspaceMask, __instance._clear2DTexCommandBuffer);
                for (int i = 0; i < 6; i++)
                {
                    //These can probably run in parallel
                    __instance._light.AddCommandBuffer(LightEvent.BeforeScreenspaceMask, __instance._faceCommandBuffers[i]);
                }
                __instance._light.AddCommandBuffer(LightEvent.AfterShadowMap, __instance._paramsCommandBuffer);
                return false;
            }

            private static bool Pre_CubeLight_RemoveCommandBuffers(CubeLight __instance)
            {
                if (__instance._faceCommandBuffers == null)
                {
                    return false;
                }
                __instance._light.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, __instance._clear2DTexCommandBuffer);
                for (int i = 0; i < 6; i++)
                {
                    if (__instance._faceCommandBuffers[i] != null)
                    {
                        __instance._light.RemoveCommandBuffer(LightEvent.BeforeScreenspaceMask, __instance._faceCommandBuffers[i]);
                    }
                }
                __instance._light.RemoveCommandBuffer(LightEvent.AfterShadowMap, __instance._paramsCommandBuffer);
                return false;
            }
        }
    }
}
