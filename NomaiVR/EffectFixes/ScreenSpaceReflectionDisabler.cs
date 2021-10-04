using System;
using System.Collections.Generic;
using System.Text;

namespace NomaiVR
{
    internal class ScreenSpaceReflectionDisabler : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ScreenSpaceReflectionDisabler.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Postfix<PostProcessingGameplaySettings>(nameof(PostProcessingGameplaySettings.ApplySettings), nameof(DisableScreenSpaceReflections));
            }

            public static void DisableScreenSpaceReflections(PostProcessingGameplaySettings __instance)
            {
                __instance._runtimeProfile.screenSpaceReflection.enabled = false;
            }
        }
    }
}
