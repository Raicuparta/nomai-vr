using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class GesturesTutorial : NomaiVRModule<GesturesTutorial.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {
                var canvas = new GameObject().AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.SetParent(Camera.main.transform, false);
                canvas.transform.localPosition += Vector3.forward * 4;
                canvas.transform.localScale = Vector3.one * 0.002f;

                var text = new GameObject().AddComponent<Text>();
                text.color = Color.white;
                text.text = "Hello world";
                text.transform.SetParent(canvas.transform, false);
                text.fontSize = 50;
                text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.alignment = TextAnchor.MiddleCenter;

                text.material = new Material(text.material);
                MaterialHelper.MakeMaterialDrawOnTop(text.material);
            }
        }
    }
}
