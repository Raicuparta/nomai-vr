using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

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

        public static void MakeGraphicDrawOnTop(Graphic graphic)
        {
            if (graphic.material == Canvas.GetDefaultCanvasMaterial())
            {
                graphic.material = new Material(graphic.material);
            }
            MakeMaterialDrawOnTop(graphic.material);
        }

        public static void MakeGraphicChildrenDrawOnTop(GameObject parent)
        {
            var graphics = parent.GetComponentsInChildren<Graphic>(true);
            foreach (var graphic in graphics)
            {
                MakeGraphicDrawOnTop(graphic);
            }
        }
    }
}
