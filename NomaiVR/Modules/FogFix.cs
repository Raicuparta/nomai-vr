using OWML.Common;
using UnityEngine;
using OWML.ModHelper.Events;
using UnityEngine.XR;
using System.Reflection;

namespace Raicuparta.NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        private void Start() {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("ResetFogSettings", typeof(Patches), "PatchResetFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), "PatchUpdateFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), "PatchOverrideFog");
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