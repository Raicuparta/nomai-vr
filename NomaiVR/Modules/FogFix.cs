using UnityEngine;

namespace NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        static protected bool resetFog = false;
        static protected bool updateFog = false;
        static protected bool overrideFog = false;

        private void Start() {
            // This might almost work, but then the other cameras (like the probe) will affect the counts.
            // So I'm disabling for now.
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("ResetFogSettings", typeof(Patches), "PatchResetFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlanetaryFogController>("UpdateFogSettings", typeof(Patches), "PatchUpdateFog");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<FogOverrideVolume>("OverrideFogSettings", typeof(Patches), "PatchOverrideFog");

            // This just prevents the fog from resetting. Looks fine I think?
            //NomaiVR.Helper.HarmonyHelper.EmptyMethod<PlanetaryFogController>("ResetFogSettings");
        }

        void Update () {
            if (Input.GetKeyDown(KeyCode.J)) {
                resetFog = !resetFog;
            }
            if (Input.GetKeyDown(KeyCode.K)) {
                updateFog = !updateFog;
            }
            if (Input.GetKeyDown(KeyCode.L)) {
                overrideFog = !overrideFog;
            }
            if (Input.anyKeyDown) {
                NomaiVR.Log("reset " + resetFog + ", update " + updateFog + ", override " + overrideFog);
            }
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