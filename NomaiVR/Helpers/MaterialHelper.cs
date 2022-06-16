using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NomaiVR.Helpers
{
    public static class MaterialHelper
    {
        private static Material overlayMaterial;

        public static Material GetOverlayMaterial()
        {
            if (overlayMaterial == null)
            {
                overlayMaterial = new Material(Canvas.GetDefaultCanvasMaterial());
                MakeMaterialDrawOnTop(overlayMaterial);
            }
            return overlayMaterial;
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

        public static void ReplaceShadersInChildren(GameObject parent, Dictionary<Shader, Shader> shaderMap)
        {
            var renderers = parent.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                foreach(var mat in renderer.materials)
                {
                    if (shaderMap.ContainsKey(mat.shader))
                        mat.shader = shaderMap[mat.shader];
                }
            }
        }

        public static void DisableRenderersWithShaderInChildren(GameObject parent, Shader shader)
        {
            var renderers = parent.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if(renderer.materials.Any(m => m.shader == shader))
                {
                    renderer.gameObject.SetActive(false);
                }
            }
        }

        public static void ApplyCanvasOpacity(Canvas canvas, float alpha)
        {
            CanvasGroup opacityGroup = canvas.GetComponent<CanvasGroup>();
            if (opacityGroup == null)
                opacityGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            opacityGroup.alpha = alpha;

            foreach (Renderer renderer in canvas.GetComponentsInChildren<Renderer>(true))
            {
                var uiColor = renderer.material.color;
                uiColor.a = alpha; //Squared for more drastic changes
                renderer.material.SetColor("_Color", uiColor);
            }
        }
    }
}
