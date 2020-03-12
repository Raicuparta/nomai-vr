using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class Dialog: MonoBehaviour {

        private static CharacterDialogueTree currentDialogue;
        private static Transform canvasTransform;
        // distance away from the player the dialog renders
        private const float dialogRenderDistance = 1f;
        private const float dialogRenderSize = 0.0015f;

        void Start () {
            NomaiVR.Log("Started Dialog Fixes");

            currentDialogue = null;
            canvasTransform = GameObject.Find("DialogueCanvas").transform;

            if (canvasTransform == null) {
                NomaiVR.Log("Couldn't find canvas with name: DialogueCanvas");
            } else {
                canvasTransform.localScale *= dialogRenderSize;
            }

            var canvas = canvasTransform.GetComponentInChildren<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }

        void Update () {
            if ((currentDialogue != null) && (canvasTransform != null)) {
                Transform attentionPoint = currentDialogue.GetValue<Transform>("_attentionPoint");
                canvasTransform.parent = attentionPoint;
                canvasTransform.localPosition = Vector3.zero;
                canvasTransform.LookAt(2 * attentionPoint.position - Camera.main.transform.position, Common.PlayerHead.up);

                // Move so it is 1 unit away from the player
                float distance = Vector3.Distance(attentionPoint.position, Camera.main.transform.position);
                canvasTransform.position = Vector3.MoveTowards(attentionPoint.position, Camera.main.transform.position, distance - dialogRenderDistance);
            }
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<CharacterDialogueTree>("StartConversation", typeof(Patches), nameof(Patches.PatchStartConversation));
                NomaiVR.Pre<CharacterDialogueTree>("EndConversation", typeof(Patches), nameof(Patches.PatchEndConversation));
            }
            static void PatchStartConversation (CharacterDialogueTree __instance) {
                currentDialogue = __instance;
            }
            static void PatchEndConversation () {
                currentDialogue = null;
            }
        }
    }


}
