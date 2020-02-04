using UnityEngine;

namespace NomaiVR {
    public class HoldHUD: MonoBehaviour {
        void Awake () {
            // Move helmet forward to make it a bit more visible.
            FindObjectOfType<HUDHelmetAnimator>().transform.localPosition += Vector3.forward * 0.2f;

            var playerHUD = GameObject.Find("PlayerHUD");
            playerHUD.transform.localScale = Vector3.one * 0.2f;
            playerHUD.transform.localPosition = Vector3.zero;
            playerHUD.transform.localRotation = Quaternion.identity;

            var hudElements = Common.GetObjectsInLayer(playerHUD.gameObject, LayerMask.NameToLayer("HeadsUpDisplay"));

            foreach (var hudElement in hudElements) {
                hudElement.layer = 0;
                hudElement.SetActive(true);
            }

            var uiCanvas = playerHUD.transform.Find("HelmetOnUI/UICanvas").GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.transform.localPosition = Vector3.zero;
            uiCanvas.transform.localRotation = Quaternion.identity;

            Hands.HoldObject(playerHUD.transform, Hands.LeftHand, new Vector3(0.12f, -0.09f, 0.01f), Quaternion.Euler(47f, 220f, 256f));

            playerHUD.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRender;
        }

        bool ShouldRender () {
            return Locator.GetPlayerSuit().IsWearingSuit(true);
        }
    }
}
