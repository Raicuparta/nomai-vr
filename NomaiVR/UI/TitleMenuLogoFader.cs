using System;
using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR.UI
{
    /// <summary>
    /// Hacky component to prevent the title logo screen being akwardly visible
    /// </summary>
    internal class TitleMenuLogoFader : MonoBehaviour
    {
        private List<Material> materialsToFade;
        private float duration = -1;
        private float elapsedTime = 0.0f;
        private float fadeTo = -1;
        private bool forceOff = false;
        private Func<bool> activationFunc;
        private Func<float, float> fadeFunc;

        internal void Start()
        {
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);

            materialsToFade = new List<Material>();
            foreach(Renderer renderer in childRenderers)
                materialsToFade.Add(renderer.material);
        }

        internal void OnEnable()
        {
            elapsedTime = 0;
            FadeTo(0);
        }

        internal void OnDisable()
        {
            elapsedTime = duration;
            FadeTo(fadeTo);
        }

        public void BeginFade(float fade, float duration, Func<bool> activationFunc, Func<float, float> fadeFunc = null, bool forceOff = false)
        {
            fadeTo = fade;
            this.duration = duration;
            this.fadeFunc = fadeFunc;
            this.activationFunc = activationFunc;
            this.forceOff = forceOff;
            this.enabled = true;
        }

        private void FadeTo(float value)
        {
            materialsToFade?.ForEach(m =>  m.SetAlpha(value));
        }

        internal void Update()
        {
            if(duration >= 0 && activationFunc == null)
            {
                elapsedTime += Time.deltaTime;
                float percentage = elapsedTime / duration;

                if (fadeFunc != null)
                    percentage = fadeFunc.Invoke(percentage);

                FadeTo(fadeTo * Mathf.Clamp(percentage, 0, 1));

                if (percentage >= 1)
                    this.enabled = false;
            }
            else if(activationFunc != null && activationFunc.Invoke())
            {
                activationFunc = null;
                OnEnable();
            }
            else if(forceOff)
            {
                FadeTo(0);
            }
        }
    }
}
