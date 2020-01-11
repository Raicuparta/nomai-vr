using OWML.Common;
using UnityEngine;
using OWML.ModHelper.Events;

namespace Raicuparta.NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        private void Start() {
            //NomaiVR.Helper.Events.Subscribe<FogWarpVolume>(Events.AfterAwake);
            NomaiVR.Helper.Events.Subscribe<PlanetaryFogController>(Events.AfterEnable);
            NomaiVR.Helper.Events.Subscribe<FogOverrideVolume>(Events.AfterAwake);
            NomaiVR.Helper.Events.OnEvent += OnEvent;
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            behaviour.SetValue("_fogTint", Color.clear);
            if (behaviour.GetType() == typeof(AnglerfishController) && ev == Events.AfterEnable) {
                NomaiVR.Log("Deactivating anglerfish");
                behaviour.gameObject.SetActive(false);
            } else if (behaviour.GetType().IsSubclassOf(typeof(FogWarpVolume)) && ev == Events.AfterAwake) {
                NomaiVR.Log("Clearing _fogColor in FogWarpVolume");
                behaviour.SetValue("_fogColor", Color.clear);
            } else if (behaviour.GetType() == typeof(PlanetaryFogController) && ev == Events.AfterEnable) {
                NomaiVR.Log("Clearing _fogTint in PlanetaryFogController");
                behaviour.SetValue("_fogTint", Color.clear);
            } else if (behaviour.GetType() == typeof(FogOverrideVolume) && ev == Events.AfterAwake) {
                NomaiVR.Log("Clearing _tint in FogOverrideVolume");
                behaviour.SetValue("_tint", Color.clear);
            }
        }
    }

}