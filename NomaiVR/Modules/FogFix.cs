using UnityEngine;

namespace NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        private void Start() {
            // This might almost work, but then the other cameras (like the probe) will affect the counts.
            // So I'm disabling for now.
            //NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("ResetFogSettings", typeof(Patches), "PatchResetFog");
            //NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), "PatchUpdateFog");
            //NomaiVR.Helper.HarmonyHelper.AddPrefix<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), "PatchOverrideFog");

            // This just prevents the fog from resetting. Looks fine I think?
            NomaiVR.Helper.HarmonyHelper.EmptyMethod<PlanetaryFogController>("ResetFogSettings");
        }
    }

    internal static class Patches
    {
        static bool _isEvenFrame = true;

        static bool PatchResetFog() {
            _isEvenFrame = !_isEvenFrame;
            return _isEvenFrame;
        }
        static bool PatchUpdateFog() {
            return _isEvenFrame;
        }
        static bool PatchOverrideFog() {
            return !_isEvenFrame;
        }
    }

}