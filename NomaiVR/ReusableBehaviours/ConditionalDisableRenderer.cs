using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ConditionalDisableRenderer : ConditionalRenderer
    {
        private Renderer _renderer;
        internal void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        protected override void SetShow(bool show)
        {
            base.SetShow(show);
            _renderer.enabled = show;
        }
    }
}
