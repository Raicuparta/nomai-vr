using UnityEngine;

namespace NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        private void Start() {
            NomaiVR.Log("Started FogFix");

            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("ResetFogSettings", typeof(Patches), "PatchResetFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), "PatchUpdateFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), "PatchOverrideFog");
        }

        internal static class Patches
        {
            static bool PatchResetFog() {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;
            }
            static bool PatchUpdateFog() {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }
            static bool PatchOverrideFog() {
                return Camera.current.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right;
            }
        }
    }

}