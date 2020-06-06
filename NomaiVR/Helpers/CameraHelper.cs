using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    public static class CameraHelper
    {
        public static OWCamera CopyCamera(OWCamera originalOWCamera)
        {
            var originalCamera = originalOWCamera.gameObject.GetComponent<Camera>();

            var cameraObject = new GameObject();
            cameraObject.SetActive(false);
            //cameraObject.transform.parent = originalOWCamera.transform;
            cameraObject.transform.localPosition = originalOWCamera.transform.position;
            cameraObject.transform.localRotation = originalOWCamera.transform.rotation;

            var camera = cameraObject.AddComponent<Camera>();
            camera.farClipPlane = originalCamera.farClipPlane;
            camera.clearFlags = originalCamera.clearFlags;
            camera.backgroundColor = originalCamera.backgroundColor;
            camera.cullingMask = originalCamera.cullingMask;
            camera.depth = originalCamera.depth;
            camera.tag = originalCamera.tag;

            var owCamera = cameraObject.AddComponent<OWCamera>();
            owCamera.renderSkybox = originalOWCamera.renderSkybox;

            var flashbackEffect = cameraObject.AddComponent<FlashbackScreenGrabImageEffect>();
            flashbackEffect._downsampleShader = cameraObject.GetComponent<FlashbackScreenGrabImageEffect>()._downsampleShader;

            cameraObject.AddComponent<FlareLayer>();

            cameraObject.SetActive(true);
            return owCamera;
        }
    }
}
