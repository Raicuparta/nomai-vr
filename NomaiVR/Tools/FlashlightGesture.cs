using UnityEngine;

namespace NomaiVR
{
    public class FlashlightGesture : NomaiVRModule<FlashlightGesture.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            void Start()
            {
                var detector = Locator.GetPlayerCamera().gameObject.AddComponent<ProximityDetector>();
                detector.other = HandsController.Behaviour.RightHand;
                detector.localOffset = Vector3.right * 0.15f;
                detector.onEnter += FlashlightPress;
                detector.onExit += FlashlightRelease;

                var flashLight = Locator.GetFlashlight();
                var pivot = flashLight.transform.Find("Flashlight_BasePivot/Flashlight_WobblePivot");
                var fillLight = pivot.Find("Flashlight_FillLight");
                var spotLight = pivot.Find("Flashlight_SpotLight");

                fillLight.localPosition = spotLight.localPosition = flashLight.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
                fillLight.localRotation = spotLight.localRotation = flashLight.transform.localRotation = Quaternion.identity;
            }

            void FlashlightPress()
            {
                ControllerInput.Behaviour.SimulateInput(XboxButton.RightStickClick, 1);
            }
            void FlashlightRelease()
            {
                ControllerInput.Behaviour.SimulateInput(XboxButton.RightStickClick, 0);
            }
        }
    }
}