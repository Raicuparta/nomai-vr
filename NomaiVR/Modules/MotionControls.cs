using OWML.Common;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace NomaiVR
{
    public class MotionControls : MonoBehaviour
    {
        void Start() {
            NomaiVR.Log("Start MotionControls");

            NomaiVR.Helper.Events.Subscribe<Flashlight>(Events.AfterStart);
            NomaiVR.Helper.Events.OnEvent += OnWakeUp;
        }

        private void OnWakeUp(MonoBehaviour behaviour, Events ev) {
            var signalScope = GameObject.Find("Signalscope");
            signalScope.transform.parent = Common.MainCamera.transform.parent;
            signalScope.transform.position = Common.PlayerHead.position;
            signalScope.transform.localRotation = Quaternion.identity;

            var signalScopeModel = signalScope.transform.GetChild(0);

            // Tools have a special shader that draws them on top of everything
            // and screws with perspective. Changing to Standard shader so they look
            // like a normal 3D object.
            signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
            signalScopeModel.localPosition = Vector3.up * -0.1f;
            signalScopeModel.localRotation = Quaternion.identity;

            // This child seems to be only for some kind of shader effect.
            // Disabling it since it doesn't seem necessary.
            signalScopeModel.GetChild(0).gameObject.SetActive(false);

            var poseDriver = signalScope.AddComponent<TrackedPoseDriver>();
            poseDriver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);
            poseDriver.UseRelativeTransform = true;

            // Attatch Signalscope UI to the Signalscope.
            var reticule = GameObject.Find("SignalscopeReticule").GetComponent<Canvas>();
            reticule.renderMode = RenderMode.WorldSpace;
            reticule.transform.parent = signalScope.transform;
            reticule.transform.localScale = Vector3.one * 0.0005f;
            reticule.transform.localPosition = Vector3.forward * 0.5f;
            reticule.transform.localRotation = Quaternion.identity;

        }

    }
}
