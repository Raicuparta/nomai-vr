using NomaiVR.Assets;
using NomaiVR.Helpers;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours.Dream
{
    /// <summary>
    /// Support camera used for stereo rendering of the simulation effect
    /// </summary>
    public class SupportSimulationCamera : MonoBehaviour
    {
        private Camera camera;
        private SimulationCamera simulationCamera;
        private RenderTexture simulationRenderTexture;

        private void Awake()
        {
            this.camera = gameObject.AddComponent<Camera>();
            this.camera.stereoTargetEye = StereoTargetEyeMask.Right;
            this.simulationRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.sRGB);
            this.simulationRenderTexture.name = "SimulationRenderTexture_R";
            this.simulationRenderTexture.useDynamicScale = true;
            this.enabled = false;
        }

        public void SetupSimulationCameraParent(SimulationCamera simulationCamera)
        {
            this.simulationCamera = simulationCamera;
            this.gameObject.layer = simulationCamera.gameObject.layer;
            this.camera.cullingMask = simulationCamera._camera.cullingMask;
            this.camera.depthTextureMode = DepthTextureMode.Depth;
            this.camera.allowMSAA = false;
            this.camera.clearFlags = simulationCamera._camera.clearFlags;
            this.camera.backgroundColor = simulationCamera._camera.backgroundColor;
            this.camera.renderingPath = simulationCamera._camera.renderingPath;
            this.camera.depth = simulationCamera._camera.depth;
            this.camera.nearClipPlane = simulationCamera._camera.nearClipPlane;
            this.camera.farClipPlane = simulationCamera._camera.farClipPlane;
            this.camera.allowDynamicResolution = simulationCamera._camera.allowDynamicResolution;
            simulationCamera._simulationMaskMaterial.shader = ShaderLoader.GetShader("Hidden/StereoBlitSimulationMask");
            simulationCamera._simulationCompositeMaterial.shader = ShaderLoader.GetShader("Hidden/StereoBlitSimulationComposite");
            simulationCamera._simulationMaskMaterial.SetTexture("_RightTex", this.simulationRenderTexture);
            simulationCamera._simulationCompositeMaterial.SetTexture("_RightTex", this.simulationRenderTexture);
        }

        private void OnEnable()
        {
            this.camera.enabled = true;
        }

        private void OnDisable()
        {
            this.camera.enabled = false;
        }

        public void AllocateTexture()
        {
            this.simulationRenderTexture.Create();
            this.camera.targetTexture = this.simulationRenderTexture;
        }

        public void DeallocateTexture()
        {
            this.camera.targetTexture = null;
            this.simulationRenderTexture.Release();
        }

        private void OnDestroy()
        {
            this.simulationRenderTexture.Release();
            GameObject.Destroy(this.simulationRenderTexture);
            this.simulationRenderTexture = null;
        }

        public void VerifyRenderTexResolution(Camera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }
            if (this.simulationRenderTexture.width == targetCamera.pixelWidth && this.simulationRenderTexture.height == targetCamera.pixelHeight)
            {
                return;
            }
            if (this.simulationRenderTexture.IsCreated())
            {
                this.simulationRenderTexture.Release();
                this.simulationRenderTexture.width = targetCamera.pixelWidth;
                this.simulationRenderTexture.height = targetCamera.pixelHeight;
                this.simulationRenderTexture.Create();
                return;
            }
            this.simulationRenderTexture.width = targetCamera.pixelWidth;
            this.simulationRenderTexture.height = targetCamera.pixelHeight;
        }

        private void OnPreRender()
        {
            if (this.simulationCamera == null || this.simulationCamera._targetCamera == null) return;
            GraphicsHelper.ForceCameraToEye(this.camera, this.simulationCamera._targetCamera.mainCamera, Valve.VR.EVREye.Eye_Right);
        }
    }
}