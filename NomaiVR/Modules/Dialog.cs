using UnityEngine;
using OWML.ModHelper.Events;
using System.Collections;

namespace NomaiVR {
    public class Dialog: MonoBehaviour {

        void Start () {
            NomaiVR.Log("Started Dialog Fixes");

            NomaiVR.Helper.HarmonyHelper.AddPrefix<CharacterDialogueTree>("StartConversation", typeof(Patches), "PatchStartConversation");
        }

        internal static class Patches {
            static void PatchStartConversation (CharacterDialogueTree __instance) {
                GameObject dialogCanvas = GameObject.Find(Menus.CanvasTypes.DialogueCanvas);
                if (dialogCanvas == null) {
                    NomaiVR.Log("Couldn't find canvas with name: " + Menus.CanvasTypes.DialogueCanvas);
                    return;
                }

                Transform attentionPoint = __instance.GetValue<Transform>("_attentionPoint");
                dialogCanvas.transform.parent = attentionPoint;
                dialogCanvas.transform.localPosition = Vector3.zero;
                dialogCanvas.transform.LookAt(2 * attentionPoint.position - Common.MainCamera.transform.position, Common.PlayerHead.up);
                
                // Move so it is 1 unit away from the player
                float distance = Vector3.Distance(attentionPoint.position, Common.MainCamera.transform.position);
                dialogCanvas.transform.position = Vector3.MoveTowards(attentionPoint.position, Common.MainCamera.transform.position, distance - 1f);
            }
        }
    }
}
