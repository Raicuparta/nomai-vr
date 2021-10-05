using NomaiVR.ReusableBehaviours;
using UnityEngine;

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
                // Prefix<MindProjectorImageEffect>(nameof(MindProjectorImageEffect.Awake), nameof(ChangeProjector));
                // Prefix<MindProjectorImageEffect>(nameof(MindProjectorImageEffect.OnRenderImage), nameof(BlitImageEffect));
                // Prefix<MindProjectorImageEffect>("set_eyeOpenness", nameof(SetEyeOpennes));
                // Prefix<MindProjectorImageEffect>("set_slideFade", nameof(SetSlideFade));
                // Prefix<MindProjectorImageEffect>("set_slideTexture", nameof(SetSlideTexture));
            }

            public static void DisableScreenSpaceReflections(PostProcessingGameplaySettings __instance)
            {
                __instance._runtimeProfile.screenSpaceReflection.enabled = false;
            }

            public static bool BlitImageEffect(RenderTexture source, RenderTexture destination)
            {
                Graphics.Blit(source, destination);
                return false;
            }

            public static bool ChangeProjector(MindProjectorImageEffect __instance)
            {
                var projector = __instance.gameObject.AddComponent<VRMindProjectorImageEffect>();
                projector.enabled = false;
                return false;
            }

            public static bool SetEyeOpennes(MindProjectorImageEffect __instance, float value)
            {
                var vrProjector = __instance.GetComponent<VRMindProjectorImageEffect>();
                if(!vrProjector.enabled) vrProjector.enabled = true;
                vrProjector.eyeOpenness = value;
                return false;
            }

            public static bool SetSlideFade(MindProjectorImageEffect __instance, float value)
            {
                __instance.GetComponent<VRMindProjectorImageEffect>().slideFade = value;
                return false;
            }

            public static bool SetSlideTexture(MindProjectorImageEffect __instance, Texture value)
            {
                var vrProjector = __instance.GetComponent<VRMindProjectorImageEffect>();
                vrProjector.slideTexture = value;
                if (value == null) vrProjector.enabled = false;
                return false;
            }
        }
    }
}
