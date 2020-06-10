using UnityEngine;

namespace NomaiVR
{
    internal class PostCreditsFix : NomaiVRModule<PostCreditsFix.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => new[] { OWScene.PostCreditsScene };

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {
                var camera = Instantiate(AssetLoader.PostCreditsPrefab);
                camera.transform.GetChild(0).parent = null;

                var originalCamera = Camera.main;
                originalCamera.tag = "Untagged";
                originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UNUSED"));
                originalCamera.transform.position = new Vector3(1075, 505, -765);
                originalCamera.transform.rotation = Quaternion.identity;
                originalCamera.rect.Set(0, 0, 1, 0.5f);
                originalCamera.targetTexture = AssetLoader.PostCreditsRenderTexture;
            }
        }
    }
}
