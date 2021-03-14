using OWML.Utils;
using UnityEngine;

namespace NomaiVR
{
    internal class WaterFix : NomaiVRModule<WaterFix.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {
                // Disable underwater distortion.
                FindObjectOfType<UnderwaterEffectBubbleController>().gameObject.SetActive(false);

                // Disable water entering and exiting effect.
                var visorEffects = FindObjectOfType<VisorEffectController>();
                visorEffects._waterClearLength = 0;
                visorEffects._waterFadeInLength = 0;
            }
        }
    }
}
