using UnityEngine;

namespace NomaiVR {
    class FlashlightGesture: MonoBehaviour {
        void Awake () {
            var detector = Locator.GetPlayerCamera().gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
            detector.localOffset = Vector3.right * 0.15f;
            detector.onEnter += FlashlightPress;
            detector.onExit += FlashlightRelease;

            var flashLight = Locator.GetFlashlight().transform;
            var pivot = flashLight.Find("Flashlight_BasePivot/Flashlight_WobblePivot");
            var fillLight = pivot.Find("Flashlight_FillLight");
            var spotLight = pivot.Find("Flashlight_SpotLight");

            fillLight.localPosition = spotLight.localPosition = flashLight.localPosition = Vector3.zero;
            fillLight.localRotation = spotLight.localRotation = flashLight.localRotation = Quaternion.identity;
        }

        void FlashlightPress () {
            ControllerInput.SimulateInput(XboxButton.RightStickClick, 1);
        }
        void FlashlightRelease () {
            ControllerInput.SimulateInput(XboxButton.RightStickClick, 0);
        }
    }
}
