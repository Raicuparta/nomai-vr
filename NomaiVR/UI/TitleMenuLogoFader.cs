using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    /// <summary>
    /// Hacky component to prevent the title logo screen being akwardly visible
    /// </summary>
    internal class TitleMenuLogoFader : MonoBehaviour
    {
        private List<Material> _materialsToFade;
        private float _duration = -1;
        private float _elapsedTime = 0.0f;
        private float _fadeTo = -1;
        private bool _forceOff = false;
        private Func<bool> _activationFunc;
        private Func<float, float> _fadeFunc;

        internal void Start()
        {
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);

            _materialsToFade = new List<Material>();
            foreach(Renderer renderer in childRenderers)
                _materialsToFade.Add(renderer.material);
        }

        internal void OnEnable()
        {
            _elapsedTime = 0;
            FadeTo(0);
        }

        internal void OnDisable()
        {
            _elapsedTime = _duration;
            FadeTo(_fadeTo);
        }

        public void BeginFade(float fade, float duration, Func<bool> activationFunc, Func<float, float> fadeFunc = null, bool forceOff = false)
        {
            _fadeTo = fade;
            _duration = duration;
            _fadeFunc = fadeFunc;
            _activationFunc = activationFunc;
            _forceOff = forceOff;
            this.enabled = true;
        }

        private void FadeTo(float value)
        {
            _materialsToFade?.ForEach(m =>  m.SetAlpha(value));
        }

        internal void Update()
        {
            if(_duration >= 0 && _activationFunc == null)
            {
                _elapsedTime += Time.deltaTime;
                float percentage = _elapsedTime / _duration;

                if (_fadeFunc != null)
                    percentage = _fadeFunc.Invoke(percentage);

                FadeTo(_fadeTo * Mathf.Clamp(percentage, 0, 1));

                if (percentage >= 1)
                    this.enabled = false;
            }
            else if(_activationFunc != null && _activationFunc.Invoke())
            {
                _activationFunc = null;
                OnEnable();
            }
            else if(_forceOff)
            {
                FadeTo(0);
            }
        }
    }
}
