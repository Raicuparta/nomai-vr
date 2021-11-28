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
            camera = gameObject.AddComponent<Camera>();
            camera.stereoTargetEye = StereoTargetEyeMask.Right;
            simulationRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.sRGB);
            simulationRenderTexture.name = "SimulationRenderTexture_R";
            simulationRenderTexture.useDynamicScale = true;
            enabled = false;
        }

        public void SetupSimulationCameraParent(SimulationCamera simulationCamera)
        {
            this.simulationCamera = simulationCamera;
            gameObject.layer = simulationCamera.gameObject.layer;
            camera.cullingMask = simulationCamera._camera.cullingMask;
            camera.depthTextureMode = DepthTextureMode.Depth;
            camera.allowMSAA = false;
            camera.clearFlags = simulationCamera._camera.clearFlags;
            camera.backgroundColor = simulationCamera._camera.backgroundColor;
            camera.renderingPath = simulationCamera._camera.renderingPath;
            camera.depth = simulationCamera._camera.depth;
            camera.nearClipPlane = simulationCamera._camera.nearClipPlane;
            camera.farClipPlane = simulationCamera._camera.farClipPlane;
            camera.allowDynamicResolution = simulationCamera._camera.allowDynamicResolution;
            simulationCamera._simulationMaskMaterial.shader = ShaderLoader.GetShader("Hidden/StereoBlitSimulationMask");
            simulationCamera._simulationCompositeMaterial.shader = ShaderLoader.GetShader("Hidden/StereoBlitSimulationComposite");
            simulationCamera._simulationMaskMaterial.SetTexture("_RightTex", simulationRenderTexture);
            simulationCamera._simulationCompositeMaterial.SetTexture("_RightTex", simulationRenderTexture);
        }

        private void OnEnable()
        {
            camera.enabled = true;
            GraphicsHelper.SetCameraEyeProjectionMatrix(camera, Valve.VR.EVREye.Eye_Right);
        }

        private void OnDisable()
        {
            camera.enabled = false;
        }

        public void AllocateTexture()
        {
            simulationRenderTexture.Create();
            camera.targetTexture = simulationRenderTexture;
        }

        public void DeallocateTexture()
        {
            camera.targetTexture = null;
            simulationRenderTexture.Release();
        }

        private void OnDestroy()
        {
            simulationRenderTexture.Release();
            Destroy(simulationRenderTexture);
            simulationRenderTexture = null;
        }

        public void VerifyRenderTexResolution(Camera targetCamera)
        {
            if (targetCamera == null)
            {
                return;
            }
            if (simulationRenderTexture.width == targetCamera.pixelWidth && simulationRenderTexture.height == targetCamera.pixelHeight)
            {
                return;
            }
            if (simulationRenderTexture.IsCreated())
            {
                simulationRenderTexture.Release();
                simulationRenderTexture.width = targetCamera.pixelWidth;
                simulationRenderTexture.height = targetCamera.pixelHeight;
                simulationRenderTexture.Create();
                return;
            }
            simulationRenderTexture.width = targetCamera.pixelWidth;
            simulationRenderTexture.height = targetCamera.pixelHeight;
        }

        private void OnPreRender()
        {
            if (simulationCamera == null || simulationCamera._targetCamera == null) return;
            GraphicsHelper.ForceCameraToEye(camera, simulationCamera._targetCamera.mainCamera.transform, Valve.VR.EVREye.Eye_Right);
        }
    }
}