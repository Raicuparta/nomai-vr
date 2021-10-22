using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.Tools
{
    internal class FlashlightGesture : NomaiVRModule<FlashlightGesture.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {
                SetupDetector(HandsController.Behaviour.RightHand, Vector3.right * 0.15f);
                SetupDetector(HandsController.Behaviour.LeftHand, Vector3.left * 0.15f);

                var flashLight = Locator.GetFlashlight();
                var pivot = flashLight.transform.Find("Flashlight_BasePivot/Flashlight_WobblePivot");
                var fillLight = pivot.Find("Flashlight_FillLight");
                var spotLight = pivot.Find("Flashlight_SpotLight");

                fillLight.localPosition = spotLight.localPosition = flashLight.transform.localPosition = new Vector3(0, 0.05f, 0.05f);
                fillLight.localRotation = spotLight.localRotation = flashLight.transform.localRotation = Quaternion.identity;
            }

            private void SetupDetector(Transform hand, Vector3 offset)
            {
                var detector = Locator.GetPlayerCamera().gameObject.AddComponent<ProximityDetector>();
                detector.Other = hand;
                detector.LocalOffset = offset;
                detector.OnEnter += FlashlightPress;
                detector.OnExit += FlashlightRelease;
            }

            private void FlashlightPress(Transform hand)
            {
                ControllerInput.SimulateInput(InputConsts.InputCommandType.FLASHLIGHT, true);
            }

            private void FlashlightRelease(Transform hand)
            {
                ControllerInput.SimulateInput(InputConsts.InputCommandType.FLASHLIGHT, false);
            }
        }
    }
}