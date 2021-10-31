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
                this.enabled = false;
            }

            private void SetupDetector(Transform hand, Vector3 offset)
            {
                var detector = Locator.GetPlayerCamera().gameObject.AddComponent<ProximityDetector>();
                detector.Other = hand;
                detector.LocalOffset = offset;
                detector.OnEnter += HandEnter;
                detector.OnExit += HandExit;
            }

            private void HandEnter(Transform hand)
            {
                this.enabled = true;
                ToggleFlashLight();
            }

            private void HandExit(Transform hand)
            {
                this.enabled = false;
            }

            private void Update()
            {
                if(OWInput.IsNewlyPressed(InputLibrary.interact) || OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary))
                {
                    ToggleFlashLight();
                }
            }

            private void ToggleFlashLight()
            {
                ControllerInput.SimulateInput(InputConsts.InputCommandType.FLASHLIGHT, true);
                Invoke(nameof(ReleaseFlashLight), 0.2f);
            }

            private void ReleaseFlashLight()
            {
                ControllerInput.SimulateInput(InputConsts.InputCommandType.FLASHLIGHT, false);
            }
        }
    }
}