using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class Dialogue: MonoBehaviour {

        private Transform _canvasTransform;
        private static Transform _attentionPoint = null;
        private const float _dialogeRenderSize = 0.0015f;

        void Start () {
            NomaiVR.Log("Started Dialogue Fixes");

            _canvasTransform = GameObject.Find("DialogueCanvas").transform;

            _canvasTransform.localScale *= _dialogeRenderSize;
            _canvasTransform.parent = Locator.GetPlayerTransform();

            var canvas = _canvasTransform.gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }

        void Update () {
            if (_attentionPoint != null && _canvasTransform != null) {
                var headPosition = Common.PlayerHead.position;

                _canvasTransform.LookAt(2 * _attentionPoint.position - headPosition, Common.PlayerHead.up);

                // Move so it is 1 unit away from the player
                var offset = (_attentionPoint.position - headPosition).normalized;
                _canvasTransform.position = headPosition + offset;
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<CharacterDialogueTree>("StartConversation", typeof(Patches), nameof(Patches.PatchStartConversation));
                NomaiVR.Pre<CharacterDialogueTree>("EndConversation", typeof(Patches), nameof(Patches.PatchEndConversation));
            }
            static void PatchStartConversation (CharacterDialogueTree __instance) {
                _attentionPoint = __instance.GetValue<Transform>("_attentionPoint");
            }
            static void PatchEndConversation () {
                _attentionPoint = null;
            }
        }
    }


}
