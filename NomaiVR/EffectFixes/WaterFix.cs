using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class WaterFix : NomaiVRModule<WaterFix.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private void Start()
            {
                // Disable underwater distortion.
                FindObjectOfType<UnderwaterEffectBubbleController>().gameObject.SetActive(false);

                // Disable water entering and exiting effect.
                var visorEffects = FindObjectOfType<VisorEffectController>();
                visorEffects.SetValue("_waterClearLength", 0);
                visorEffects.SetValue("_waterFadeInLength", 0);
            }
        }
    }
}
