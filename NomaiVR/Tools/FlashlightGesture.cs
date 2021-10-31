using NomaiVR.Hands;
using NomaiVR.Input;
using NomaiVR.ReusableBehaviours;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NomaiVR.Tools
{
    internal class FlashlightGesture : NomaiVRModule<FlashlightGesture.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public static Behaviour Instance { get; private set; }

        public class Behaviour : MonoBehaviour
        {
            private List<ProximityDetector> proximityDetectors;

            internal void Awake()
            {
                Instance = this;
                proximityDetectors = new List<ProximityDetector>(2);
            }

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
                proximityDetectors.Add(detector);
            }

            public bool IsControllingFlashlight()
            {
                return proximityDetectors.Any(x => x.IsInside());
            }

            private void HandEnter(Transform hand)
            {
                ToggleFlashLight();
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