using UnityEngine;

namespace NomaiVR {
    class FeetMarker: MonoBehaviour {
        static AssetBundle _assetBundle;
        static GameObject _prefab;

        void Start () {
            if (!_assetBundle) {
                _assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/feetposition");
                _prefab = _assetBundle.LoadAsset<GameObject>("assets/feetposition.prefab");
            }

            var marker = Instantiate(_prefab).transform;
            marker.parent = Locator.GetPlayerTransform();
            marker.localPosition = -Vector3.up * Locator.GetPlayerCollider().bounds.extents.y;
            marker.localRotation = Quaternion.Euler(90, 0, 0);
            marker.localScale *= 0.75f;
            Common.ChangeLayerRecursive(marker.gameObject, "VisibleToPlayer");

            marker.GetComponentInChildren<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();
        }
    }
}
