using OWML.Utils;
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
                FixPlanetaryFogEffect();
            }

            private static void FixDarkBrambleLights()
            {
                var fogLightCanvas = GameObject.Find("FogLightCanvas").GetComponent<Canvas>();
                fogLightCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                fogLightCanvas.worldCamera = Locator.GetActiveCamera().mainCamera;
                fogLightCanvas.planeDistance = 100;
            }

            private void FixPlanetaryFogEffect()
            {
                var camera = Locator.GetPlayerCamera();
                var monoPlanetaryFog = camera.planetaryFog;
                var stereoPlanetaryFog = camera.gameObject.AddComponent<StereoPlanetaryFogImageEffect>();
                stereoPlanetaryFog.fogShader = monoPlanetaryFog.fogShader;
                camera._planetaryFog = stereoPlanetaryFog;
                Destroy(monoPlanetaryFog);
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<PlanetaryFogController>("ResetFogSettings", nameof(Patch.PatchResetFog));
                    Prefix<PlanetaryFogController>("UpdateFogSettings", nameof(Patch.PatchUpdateFog));
                    Prefix<FogOverrideVolume>("OverrideFogSettings", nameof(Patch.PatchOverrideFog));
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
