using UnityEngine;
using OWML.ModHelper.Events;

namespace NomaiVR {
    public class Dialog: MonoBehaviour {

        private static CharacterDialogueTree currentDialogue;
        private static GameObject dialogCanvas;
        // distance away from the player the dialog renders
        private static float dialogRenderDistance = 1f;
        private static float dialogRenderSize = 0.001f;

        void Start () {
            NomaiVR.Log("Started Dialog Fixes");

            NomaiVR.Helper.HarmonyHelper.AddPrefix<CharacterDialogueTree>("StartConversation", typeof(Patches), "PatchStartConversation");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<CharacterDialogueTree>("EndConversation", typeof(Patches), "PatchEndConversation");

            currentDialogue = null;
            dialogCanvas = GameObject.Find(Menus.CanvasTypes.DialogueCanvas);
            if (dialogCanvas == null) {
                NomaiVR.Log("Couldn't find canvas with name: " + Menus.CanvasTypes.DialogueCanvas);
            } else {
                dialogCanvas.transform.localScale = Vector3.one * dialogRenderSize;
            }
        }

        void Update () {
            if ((currentDialogue != null) && (dialogCanvas != null)) {
                Transform attentionPoint = currentDialogue.GetValue<Transform>("_attentionPoint");
                dialogCanvas.transform.parent = attentionPoint;
                dialogCanvas.transform.localPosition = Vector3.zero;
                dialogCanvas.transform.LookAt(2 * attentionPoint.position - Camera.main.transform.position, Common.PlayerHead.up);

                // Move so it is 1 unit away from the player
                float distance = Vector3.Distance(attentionPoint.position, Camera.main.transform.position);
                dialogCanvas.transform.position = Vector3.MoveTowards(attentionPoint.position, Camera.main.transform.position, distance - dialogRenderDistance);
            }
        }

        internal static class Patches {
            static void PatchStartConversation (CharacterDialogueTree __instance) {
                currentDialogue = __instance;
            }
            static void PatchEndConversation () {
                currentDialogue = null;
            }
        }
    }


}
