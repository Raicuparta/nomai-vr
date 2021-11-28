using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using UnityEngine;

namespace NomaiVR.Player
{
    internal class FeetMarker : NomaiVRModule<FeetMarker.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private SpriteRenderer renderer;

            internal void Start()
            {
                var marker = Instantiate(AssetLoader.FeetPositionPrefab).transform;
                marker.parent = Locator.GetPlayerTransform();
                marker.position = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2").position;
                marker.localRotation = Quaternion.Euler(90, 0, 0);
                marker.localScale *= 0.75f;
                LayerHelper.ChangeLayerRecursive(marker.gameObject, "VisibleToPlayer");

                renderer = marker.GetComponentInChildren<SpriteRenderer>();
                renderer.material = MaterialHelper.GetOverlayMaterial();

                SetFeetMarkerVisibility();

                ModSettings.OnConfigChange += SetFeetMarkerVisibility;
            }

            internal void OnDestroy()
            {
                ModSettings.OnConfigChange -= SetFeetMarkerVisibility;
            }

            private void SetFeetMarkerVisibility()
            {
                renderer.enabled = ModSettings.EnableFeetMarker;
            }
        }
    }
}
