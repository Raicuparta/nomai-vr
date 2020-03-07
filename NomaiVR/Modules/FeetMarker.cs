using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            marker.localRotation = Quaternion.identity;
        }
    }
}
