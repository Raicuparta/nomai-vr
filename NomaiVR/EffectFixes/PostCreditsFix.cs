using NomaiVR.Assets;
using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class PostCreditsFix : NomaiVRModule<PostCreditsFix.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => new[] { OWScene.PostCreditsScene };

        public class Behaviour : MonoBehaviour
        {
            private Transform screenTransform;
            private Transform cameraTransform;
            private Transform offsetTransform;

            internal void Start()
            {
                cameraTransform = Instantiate(AssetLoader.PostCreditsPrefab).transform;
                screenTransform = cameraTransform.GetChild(0);
                screenTransform.parent = null;

                offsetTransform = new GameObject("Offset").transform;
                offsetTransform.parent = cameraTransform;
                offsetTransform.localPosition = Vector3.zero;
                offsetTransform.parent = null;
                offsetTransform.rotation = Quaternion.Euler(0, -cameraTransform.rotation.eulerAngles.y, 0);

                cameraTransform.parent = offsetTransform;
                cameraTransform.localPosition = Vector3.zero;
                cameraTransform.localRotation = Quaternion.identity;

                var originalCamera = Camera.main;
                originalCamera.tag = "Untagged";
                originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UNUSED"));
                originalCamera.transform.position = new Vector3(1075, 505, -765);
                originalCamera.transform.rotation = Quaternion.identity;
                originalCamera.rect.Set(0, 0, 1, 0.5f);
                originalCamera.targetTexture = AssetLoader.PostCreditsRenderTexture;
            }

            internal void Update()
            {
                var cameraYForward = cameraTransform.forward;
                cameraYForward.y = 0;
                cameraYForward = cameraYForward.normalized;
                var signedCameraAngle = Vector3.SignedAngle(cameraYForward, -screenTransform.up, Vector3.up);
                if (Mathf.Abs(signedCameraAngle) > 70)
                {
                    offsetTransform.localRotation *= Quaternion.Euler(0, signedCameraAngle, 0);
                }
            }
        }
    }
}
