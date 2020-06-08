using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Dialogue : NomaiVRModule<Dialogue.Behaviour, Dialogue.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Transform _canvasTransform;
            private static Transform _attentionPoint = null;
            private const float _dialogeRenderSize = 0.0015f;

            private void Start()
            {
                _canvasTransform = GameObject.Find("DialogueCanvas").transform;

                _canvasTransform.localScale *= _dialogeRenderSize;
                _canvasTransform.parent = Locator.GetPlayerTransform();

                var canvas = _canvasTransform.gameObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
            }

            private void Update()
            {
                if (_attentionPoint != null && _canvasTransform != null)
                {
                    var headPosition = PlayerHelper.PlayerHead.position;

                    _canvasTransform.LookAt(2 * _attentionPoint.position - headPosition, PlayerHelper.PlayerHead.up);

                    // Move so it is 1 unit away from the player
                    var offset = (_attentionPoint.position - headPosition).normalized;
                    _canvasTransform.position = headPosition + offset;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Pre<CharacterDialogueTree>("StartConversation", typeof(Patch), nameof(PreStartConversation));
                    NomaiVR.Post<CharacterDialogueTree>("StartConversation", typeof(Patch), nameof(PostStartConversation));
                    NomaiVR.Pre<CharacterDialogueTree>("EndConversation", typeof(Patch), nameof(PreEndConversation));
                }

                private static void PreStartConversation(CharacterDialogueTree __instance)
                {
                    _attentionPoint = __instance.GetValue<Transform>("_attentionPoint");
                }

                private static void PostStartConversation()
                {
                    var graphics = _canvasTransform.gameObject.GetComponentsInChildren<Graphic>();
                    foreach (var graphic in graphics)
                    {
                        graphic.material = new Material(graphic.material);
                        MaterialHelper.MakeMaterialDrawOnTop(graphic.material);
                    }
                }

                private static void PreEndConversation()
                {
                    _attentionPoint = null;
                }
            }
        }


    }
}
