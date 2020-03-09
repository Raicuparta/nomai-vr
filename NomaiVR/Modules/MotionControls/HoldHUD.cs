using UnityEngine;

namespace NomaiVR {
    public class HoldHUD: MonoBehaviour {
        Transform _holdTransform;

        void Awake () {
            // Move helmet forward to make it a bit more visible.
            FindObjectOfType<HUDHelmetAnimator>().transform.localPosition += Vector3.forward * 0.2f;

            var playerHUD = GameObject.Find("PlayerHUD");
            var hudElements = Common.GetObjectsInLayer(playerHUD.gameObject, LayerMask.NameToLayer("HeadsUpDisplay"));

            foreach (var hudElement in hudElements) {
                hudElement.layer = 0;
            }

            var uiCanvas = playerHUD.transform.Find("HelmetOnUI/UICanvas").GetComponent<Canvas>();
            uiCanvas.transform.localScale = Vector3.one * 0.0005f;
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.transform.localPosition = Vector3.zero;
            uiCanvas.transform.localRotation = Quaternion.identity;

            _holdTransform = Hands.HoldObject(uiCanvas.transform, Hands.LeftHand, new Vector3(0.12f, -0.09f, 0.01f), Quaternion.Euler(47f, 220f, 256f));

            GlobalMessenger.AddListener("SuitUp", Enable);
            GlobalMessenger.AddListener("RemoveSuit", Disable);

            SetEnabled();
        }

        void Enable () {
            _holdTransform.gameObject.SetActive(true);
        }

        void Disable () {
            _holdTransform.gameObject.SetActive(false);
        }


        void SetEnabled () {
            if (Locator.GetPlayerSuit().IsWearingSuit(true) && Common.ToolSwapper.GetToolGroup() == ToolGroup.Suit) {
                Enable();
            } else {
                Disable();
            }
        }
    }
}
