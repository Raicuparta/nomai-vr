using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    class ButtonInteraction: MonoBehaviour {
        public XboxButton button;
        public UITextType text;
        public InteractReceiver receiver;

        void Start () {
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver.SetValue("_usableInShip", true);
            receiver.SetPromptText(text);
            receiver.OnPressInteract += OnPress;
            receiver.OnReleaseInteract += OnRelease;
        }

        void OnPress () {
            ControllerInput.SimulateInput(button, 1);
        }

        void OnRelease () {
            ControllerInput.SimulateInput(button, 0);
            receiver.ResetInteraction();
        }
    }
}
