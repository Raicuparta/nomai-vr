using OWML.Common;
using UnityEngine;
using OWML.ModHelper.Events;

namespace Raicuparta.NomaiVR
{
    public class FogFix : MonoBehaviour
    {
        OWEvent<OWCamera> _onAnyPostRender;
        OWEvent<OWCamera> _onAnyPreCull;
        int _before;
        int _after;

        private void Start() {
            NomaiVR.Helper.Events.Subscribe<PlanetaryFogController>(Events.BeforeEnable);
            NomaiVR.Helper.Events.Subscribe<PlanetaryFogController>(Events.AfterEnable);

            //NomaiVR.Helper.Events.Subscribe<FogOverrideVolume>(Events.AfterAwake);
            NomaiVR.Helper.Events.OnEvent += OnEvent;

            //NomaiVR.Helper.HarmonyHelper.EmptyMethod<PlanetaryFogController>("Awake");
            //NomaiVR.Helper.HarmonyHelper.EmptyMethod<PlanetaryFogController>("OnEnable");


            //NomaiVR.Helper.HarmonyHelper.EmptyMethod<OWCamera>("OnPostRender");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<OWCamera>("OnPostRender", typeof(void), "Patch");
        }

        static void Patch() {
            NomaiVR.Log("Post render");
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev) {
            //} else if (behaviour.GetType().IsSubclassOf(typeof(FogWarpVolume)) && ev == Events.AfterAwake) {
            //    //NomaiVR.Log("Clearing _fogColor in FogWarpVolume");
            //    behaviour.SetValue("_fogColor", Color.clear);
            if (behaviour.GetType() == typeof(PlanetaryFogController) && ev == Events.BeforeEnable) {
                //NomaiVR.Log("Before Enable PlanetaryFogController");
                //behaviour.SetValue("_fogTint", Color.clear);
                //_onAnyPostRender = OWCamera.onAnyPostRender;
                //_onAnyPreCull = OWCamera.onAnyPreCull;

                _before = OWCamera.onAnyPostRender.GetCallbacks().Length;
            } else if (behaviour.GetType() == typeof(PlanetaryFogController) && ev == Events.AfterEnable) {
                //behaviour.SetValue("_fogTint", Color.clear);
                //NomaiVR.Log("After Enable PlanetaryFogController");
                //OWCamera.onAnyPostRender -= typeof(PlanetaryFogController).GetValue<OWEvent<OWCamera>.OWCallback>("<>f__mg$cache1");

                _after = OWCamera.onAnyPostRender.GetCallbacks().Length;

                if (_before != _after) {
                    NomaiVR.Log(">>>>>>>>>>Different<<<<<<<<<< " + behaviour.GetValue<Texture3D>("_fogLookupTexture").name);
                }
                //OWCamera.onAnyPreCull = _onAnyPreCull;
            }
            //else if (behaviour.GetType() == typeof(FogOverrideVolume) && ev == Events.AfterAwake) {
            //    //NomaiVR.Log("Clearing _tint in FogOverrideVolume");
            //    behaviour.SetValue("_tint", Color.clear);
            //}
        }
    }

}