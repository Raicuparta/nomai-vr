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
        private Camera _camera;
        private SimulationCamera _simulationCamera;
        private RenderTexture _simulationRenderTexture;

        private void Awake()
        {
            this._camera = gameObject.AddComponent<Camera>();
            this._camera.stereoTargetEye = StereoTargetEyeMask.Right;
            this._simulationRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.sRGB);
            this._simulationRenderTexture.name = "SimulationRenderTexture_R";
            this._simulationRenderTexture.useDynamicScale = true;
            this.enabled = false;
        }

        public void SetupSimulationCameraParent(SimulationCamera simulationCamera)
        {
            this._simulationCamera = simulationCamera;
            this.gameObject.layer = simulationCamera.gameObject.layer;
            this._camera.cullingMask = simulationCamera._camera.cullingMask;
            this._camera.depthTextureMode = DepthTextureMode.Depth;
            this._camera.allowMSAA = false;
            this._camera.clearFlags = simulationCamera._camera.clearFlags;
            this._camera.backgroundColor = simulationCamera._camera.backgroundColor;
            this._camera.renderingPath = simulationCamera._camera.renderingPath;
            this._camera.depth = simulationCamera._camera.depth;
            this._camera.nearClipPlane = simulationCamera._camera.nearClipPlane;
            this._camera.farClipPlane = simulationCamera._camera.farClipPlane;
            this._camera.allowDynamicResolution = simulationCamera._camera.allowDynamicResolution;
            simulationCamera._simulationMaskMaterial.shader = ShaderLoader.GetShader("Hidden/StereoBlitSimulationMask");
            simulationCamera._simulationCompositeMaterial.shader = ShaderLoader.GetShader("Hidden/StereoBlitSimulationComposite");
            simulationCamera._simulationMaskMaterial.SetTexture("_RightTex", this._simulationRenderTexture);
            simulationCamera._simulationCompositeMaterial.SetTexture("_RightTex", this._simulationRenderTexture);
        }

        private void OnEnable()
        {
            this._camera.enabled = true;
        }

        private void OnDisable()
        {
            this._camera.enabled = false;
        }

        public void AllocateTexture()
        {
            this._simulationRenderTexture.Create();
            this._camera.targetTexture = this._simulationRenderTexture;
        }

        public void DeallocateTexture()
        {
            this._camera.targetTexture = null;
            this._simulationRenderTexture.Release();
        }

        private void OnDestroy()
        {
            this._simulationRenderTexture.Release();
            GameObject.Destroy(this._simulationRenderTexture);
            this._simulationRenderTexture = null;
        }

        public void VerifyRenderTexResolution(Camera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }
            if (this._simulationRenderTexture.width == targetCamera.pixelWidth && this._simulationRenderTexture.height == targetCamera.pixelHeight)
            {
                return;
            }
            if (this._simulationRenderTexture.IsCreated())
            {
                this._simulationRenderTexture.Release();
                this._simulationRenderTexture.width = targetCamera.pixelWidth;
                this._simulationRenderTexture.height = targetCamera.pixelHeight;
                this._simulationRenderTexture.Create();
                return;
            }
            this._simulationRenderTexture.width = targetCamera.pixelWidth;
            this._simulationRenderTexture.height = targetCamera.pixelHeight;
        }

        private void OnPreRender()
        {
            if (this._simulationCamera == null || this._simulationCamera._targetCamera == null) return;
            GraphicsHelper.ForceCameraToEye(this._camera, this._simulationCamera._targetCamera.mainCamera, Valve.VR.EVREye.Eye_Right);
        }
    }
}