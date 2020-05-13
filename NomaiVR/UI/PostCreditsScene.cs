using UnityEngine;

namespace NomaiVR
{
    class PostCreditsScene : MonoBehaviour
    {
        static AssetBundle _assetBundle;
        static GameObject _prefab;

        void Start()
        {
            if (!_assetBundle)
            {
                _assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/cinema-camera");
                _prefab = _assetBundle.LoadAsset<GameObject>("assets/postcreditscamera.prefab");
            }

            var camera = Instantiate(_prefab);
            camera.transform.GetChild(0).parent = null;

            var renderTexture = _assetBundle.LoadAsset<RenderTexture>("assets/screen.renderTexture");

            var originalCamera = Camera.main;
            originalCamera.tag = "Untagged";
            originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UNUSED"));
            originalCamera.transform.position = new Vector3(1075, 505, -765);
            originalCamera.transform.rotation = Quaternion.identity;
            originalCamera.rect.Set(0, 0, 1, 0.5f);
            originalCamera.targetTexture = renderTexture;

        }
    }
}
