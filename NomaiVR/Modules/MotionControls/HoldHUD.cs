using UnityEngine;

namespace NomaiVR {
    public class HoldHUD: MonoBehaviour {
        Transform _holdTransform;
        private static Transform _thrusterParent;
        private static Transform _thrusterHUD;

        void Awake () {
            SetupThrusterHUD();

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

        void SetupThrusterHUD () {
            _thrusterHUD = GameObject.Find("HUD_Thrusters").transform;
            _thrusterParent = new GameObject().transform;
            _thrusterParent.parent = _thrusterHUD.parent;
            _thrusterParent.localRotation = Quaternion.identity;
            _thrusterParent.localPosition = _thrusterHUD.localPosition;
            _thrusterHUD.parent = _thrusterParent;
            _thrusterHUD.localPosition = Vector3.zero;
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Helper.HarmonyHelper.AddPostfix<ThrustAndAttitudeIndicator>("LateUpdate", typeof(Patches), nameof(PatchLateUpdate));
            }

            static void PatchLateUpdate () {
                // only allow rotation around the up/down axis, always face forward
                _thrusterParent.transform.rotation = Quaternion.LookRotation(Common.PlayerHead.up, Locator.GetPlayerTransform().forward);
                // gets updated elsewhere and needs to be reset to proper local rotation
                _thrusterHUD.localRotation = Quaternion.Euler(-90f, 0f, 180f);
            }
        }
    }
}
