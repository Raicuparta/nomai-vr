using UnityEngine;

namespace NomaiVR
{
    public class FeetMarker : NomaiVRModule<FeetMarker.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            static AssetBundle _assetBundle;
            static GameObject _prefab;

            void Start()
            {
                if (!_assetBundle)
                {
                    _assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/feetposition");
                    _prefab = _assetBundle.LoadAsset<GameObject>("assets/feetposition.prefab");
                }

                var marker = Instantiate(_prefab).transform;
                marker.parent = Locator.GetPlayerTransform();
                marker.position = Locator.GetPlayerTransform().Find("Traveller_HEA_Player_v2").position;
                marker.localRotation = Quaternion.Euler(90, 0, 0);
                marker.localScale *= 0.75f;
                Common.ChangeLayerRecursive(marker.gameObject, "VisibleToPlayer");

                marker.GetComponentInChildren<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();
            }
        }
    }
}
