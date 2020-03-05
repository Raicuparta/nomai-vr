using OWML.ModHelper.Events;
using System.Reflection;
using UnityEngine;

namespace NomaiVR {
    class SolarSystemMap: MonoBehaviour {
        void Start () {
            var mapCameraTransform = Locator.GetRootTransform().Find("MapCamera");

            var originalCamera = mapCameraTransform.GetComponent<Camera>();
            var originalOWCamera = mapCameraTransform.GetComponent<OWCamera>();

            var newCamera = new GameObject().transform;
            newCamera.gameObject.SetActive(false);
            newCamera.parent = mapCameraTransform;
            newCamera.localPosition = Vector3.zero;
            newCamera.localRotation = Quaternion.identity;

            var camera = newCamera.gameObject.AddComponent<Camera>();
            camera.farClipPlane = 100000;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.cullingMask = originalCamera.cullingMask;
            camera.depth = 0;
            camera.enabled = false;

            var owCamera = newCamera.gameObject.AddComponent<OWCamera>();
            owCamera.renderSkybox = true;

            var flashbackEffect = newCamera.gameObject.AddComponent<FlashbackScreenGrabImageEffect>();
            flashbackEffect._downsampleShader = originalCamera.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

            newCamera.gameObject.AddComponent<FlareLayer>();

            Destroy(mapCameraTransform.GetComponent<FlareLayer>());
            Destroy(mapCameraTransform.GetComponent<FlashbackScreenGrabImageEffect>());
            Destroy(mapCameraTransform.GetComponent("PostProcessingBehaviour"));
            Destroy(originalOWCamera);
            Destroy(originalCamera);

            var mapController = mapCameraTransform.GetComponent<MapController>();

            newCamera.gameObject.SetActive(true);
            mapController.SetValue("_mapCamera", owCamera);
        }
    }
}
