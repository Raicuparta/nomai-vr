
using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class MapFix : NomaiVRModule<MapFix.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private MapController mapController;

            internal void Start()
            {
                var mapCameraTransform = Locator.GetRootTransform().Find("MapCamera");

                var originalCamera = mapCameraTransform.GetComponent<Camera>();
                var originalOwCamera = mapCameraTransform.GetComponent<OWCamera>();

                var newCamera = new GameObject("VrMapCamera").transform;
                newCamera.gameObject.SetActive(false);
                newCamera.parent = mapCameraTransform;
                newCamera.localPosition = Vector3.zero;
                newCamera.localRotation = Quaternion.identity;

                var camera = newCamera.gameObject.AddComponent<Camera>();
                camera.farClipPlane = originalCamera.farClipPlane;
                camera.clearFlags = originalCamera.clearFlags;
                camera.backgroundColor = originalCamera.backgroundColor;
                camera.cullingMask = originalCamera.cullingMask;
                camera.depth = originalCamera.depth;
                camera.tag = originalCamera.tag;
                camera.enabled = false;

                var owCamera = newCamera.gameObject.AddComponent<OWCamera>();
                owCamera.renderSkybox = true;

                var flashbackEffect = newCamera.gameObject.AddComponent<FlashbackScreenGrabImageEffect>();
                flashbackEffect._downsampleShader = originalCamera.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

                newCamera.gameObject.AddComponent<FlareLayer>();

                Destroy(mapCameraTransform.GetComponent<FlareLayer>());
                Destroy(mapCameraTransform.GetComponent<FlashbackScreenGrabImageEffect>());
                Destroy(mapCameraTransform.GetComponent("PostProcessingBehaviour"));
                Destroy(originalOwCamera);
                Destroy(originalCamera);

                mapController = mapCameraTransform.GetComponent<MapController>();

                newCamera.gameObject.SetActive(true);
                mapController._mapCamera = owCamera;

                var markerManager = mapCameraTransform.Find("MarkerManager").GetComponent<Canvas>();
                var lockOnCanvas = mapCameraTransform.Find("MapLockOnCanvas").GetComponent<Canvas>();

                markerManager.worldCamera = lockOnCanvas.worldCamera = camera;

                GlobalMessenger.AddListener("GamePaused", OnGamePaused);
            }

            internal void OnDestroy()
            {
                GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
            }

            private void OnGamePaused()
            {
                if (PlayerState.InMapView())
                {
                    mapController.ExitMapView();
                }
            }
        }
    }
}