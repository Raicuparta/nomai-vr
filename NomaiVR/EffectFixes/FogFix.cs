using UnityEngine;

namespace NomaiVR
{
    public class FogFix : NomaiVRModule<FogFix.Behaviour, FogFix.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private void Start()
            {
                FixDarkBrambleLights();
            }

            private void FixDarkBrambleLights()
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
                    NomaiVR.Pre<PlanetaryFogController>("ResetFogSettings", typeof(Patch), nameof(Patch.PatchResetFog));
                    NomaiVR.Pre<PlanetaryFogController>("UpdateFogSettings", typeof(Patch), nameof(Patch.PatchUpdateFog));
                    NomaiVR.Pre<FogOverrideVolume>("OverrideFogSettings", typeof(Patch), nameof(Patch.PatchOverrideFog));
                }

                private static bool PatchResetFog()
                {
                    return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
                }

                private static bool PatchUpdateFog()
                {
                    return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
                }

                private static bool PatchOverrideFog()
                {
                    return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
                }
            }
        }
    }
}
