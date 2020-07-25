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
                    Pre<PlanetaryFogController>("ResetFogSettings", nameof(Patch.PatchResetFog));
                    Pre<PlanetaryFogController>("UpdateFogSettings", nameof(Patch.PatchUpdateFog));
                    Pre<FogOverrideVolume>("OverrideFogSettings", nameof(Patch.PatchOverrideFog));
                }

                private static bool PatchResetFog()
                {
                    return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
                }

                private static bool PatchUpdateFog()
                {
                    if (InputHelper.IsUIInteractionMode())
                    {
                        return false;
                    }
                    return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
                }

                private static bool PatchOverrideFog()
                {
                    if (InputHelper.IsUIInteractionMode())
                    {
                        return false;
                    }
                    return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
                }
            }
        }
    }
}
