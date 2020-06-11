using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class GesturesTutorial : NomaiVRModule<GesturesTutorial.Behaviour, GesturesTutorial.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Text _text;

            internal void Start()
            {
                SetUpText();
            }

            private void SetUpText()
            {
                var canvas = new GameObject().AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.SetParent(Camera.main.transform, false);
                canvas.transform.localPosition += Vector3.forward * 4;
                canvas.transform.localScale = Vector3.one * 0.002f;

                _text = new GameObject().AddComponent<Text>();
                _text.color = Color.white;
                _text.transform.SetParent(canvas.transform, false);
                _text.fontSize = 50;
                _text.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                _text.horizontalOverflow = HorizontalWrapMode.Overflow;
                _text.alignment = TextAnchor.MiddleCenter;

                _text.material = new Material(_text.material);
                MaterialHelper.MakeMaterialDrawOnTop(_text.material);
            }

            private static void SetText(string text)
            {
                _text.text = text;
            }

            internal void LateUpdate()
            {
                var camera = Camera.main.transform;
                var hit = Physics.Raycast(camera.position, camera.forward, out var raycastHit, 75f, OWLayerMask.blockableInteractMask);
                if (!hit)
                {
                    return;
                }

                var interactReceiver = raycastHit.collider.GetComponent<InteractReceiver>();
                if (!interactReceiver)
                {
                    SetText("");
                    return;
                }
                NomaiVR.Log("Hit an interact receiver");

                SetText("Grab probe launcher from tool belt");
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    //NomaiVR.Pre<ProbePromptReceiver>("GainFocus", typeof(Patch), nameof(Patch.PreGainFocus));
                    //NomaiVR.Pre<ProbePromptReceiver>("LoseFocus", typeof(Patch), nameof(Patch.PreLoseFocus));
                }

                private static void PreGainFocus()
                {

                    NomaiVR.Log("Enter Probe Prompt Trigger");
                    SetText("Grab probe launcher from tool belt");
                }

                private static void PreLoseFocus()
                {

                    NomaiVR.Log("Exit Probe Prompt Trigger");
                    SetText("");
                }
            }
        }
    }
}
