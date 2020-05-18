using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class Dialogue : NomaiVRModule<Dialogue.Behaviour, Dialogue.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform _canvasTransform;
            private static Transform _attentionPoint = null;
            private const float _dialogeRenderSize = 0.0015f;

            void Start()
            {
                _canvasTransform = GameObject.Find("DialogueCanvas").transform;

                _canvasTransform.localScale *= _dialogeRenderSize;
                _canvasTransform.parent = Locator.GetPlayerTransform();

                var canvas = _canvasTransform.gameObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
            }

            void Update()
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
                    NomaiVR.Pre<CharacterDialogueTree>("StartConversation", typeof(Patch), nameof(Behaviour.Patch.PatchStartConversation));
                    NomaiVR.Pre<CharacterDialogueTree>("EndConversation", typeof(Patch), nameof(Behaviour.Patch.PatchEndConversation));
                }
                static void PatchStartConversation(CharacterDialogueTree __instance)
                {
                    _attentionPoint = __instance.GetValue<Transform>("_attentionPoint");
                }
                static void PatchEndConversation()
                {
                    _attentionPoint = null;
                }
            }
        }


    }
}
