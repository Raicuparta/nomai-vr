using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
{
    public class FeetMarker : NomaiVRModule<FeetMarker.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private void Start()
            {
                var marker = Instantiate(AssetLoader.FeetPositionPrefab).transform;
                marker.parent = Locator.GetPlayerTransform();
                marker.position = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2").position;
                marker.localRotation = Quaternion.Euler(90, 0, 0);
                marker.localScale *= 0.75f;
                LayerHelper.ChangeLayerRecursive(marker.gameObject, "VisibleToPlayer");

                var material = Instantiate(Canvas.GetDefaultCanvasMaterial());
                material.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
                marker.GetComponentInChildren<SpriteRenderer>().material = material;
            }
        }
    }
}
