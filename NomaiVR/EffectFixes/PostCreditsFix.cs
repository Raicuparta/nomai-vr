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
            private Camera originalCamera;
            private Camera vrCamera;
            private RenderTexture stereoTexture;

            internal void Start()
            {
                cameraTransform = Instantiate(AssetLoader.PostCreditsPrefab).transform;
                vrCamera = cameraTransform.GetComponentInChildren<Camera>();
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

                originalCamera = Camera.main;
                originalCamera.tag = "Untagged";
                originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UNUSED"));
                originalCamera.transform.position = new Vector3(1075, 505, -765);
                originalCamera.transform.rotation = Quaternion.identity;
                originalCamera.rect.Set(0, 0, 1, 0.5f);
                originalCamera.targetTexture = AssetLoader.PostCreditsRenderTexture;
                originalCamera.enabled = false;

                stereoTexture = new RenderTexture(AssetLoader.PostCreditsRenderTexture.descriptor);
                stereoTexture.Create();

                Camera.onPreRender += OnCameraPreRender;
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

                originalCamera.targetTexture = AssetLoader.PostCreditsRenderTexture; 
                originalCamera.Render();
                originalCamera.transform.position += Vector3.right * 10;
                originalCamera.targetTexture = stereoTexture;
                originalCamera.Render();
            }

            void OnCameraPreRender(Camera camera)
            {
                if (camera != vrCamera || camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right)
                    return;
                Graphics.CopyTexture(stereoTexture, AssetLoader.PostCreditsRenderTexture);
            }
        }
    }
}
