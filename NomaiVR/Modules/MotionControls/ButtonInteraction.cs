using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    class ButtonInteraction: MonoBehaviour {
        public XboxButton button;
        public UITextType text;
        InteractReceiver _interaction;

        void Start () {
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;

            _interaction = gameObject.AddComponent<InteractReceiver>();
            _interaction.SetInteractRange(2);
            _interaction.SetValue("_usableInShip", true);
            _interaction.SetPromptText(text);
            _interaction.OnPressInteract += OnPress;
            _interaction.OnReleaseInteract += OnRelease;
        }

        void OnPress () {
            ControllerInput.SimulateInput(button, 1);
        }

        void OnRelease () {
            ControllerInput.SimulateInput(button, 0);
            _interaction.ResetInteraction();
        }
    }
}
