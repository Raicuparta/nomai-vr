using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
{
    public static class MaterialHelper
    {
        private static Material _overlayMaterial;

        public static Material GetOverlayMaterial()
        {
            if (_overlayMaterial == null)
            {
                _overlayMaterial = new Material(Canvas.GetDefaultCanvasMaterial());
                MakeMaterialDrawOnTop(_overlayMaterial);
            }
            return _overlayMaterial;
        }

        public static void MakeMaterialDrawOnTop(Material material)
        {
            material.shader = Canvas.GetDefaultCanvasMaterial().shader;
            material.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
        }
    }
}
