using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR {
    class ButtonInteraction: MonoBehaviour {
        public XboxButton button;
        public UITextType text;

        void Start () {
            var collider = gameObject.AddComponent<BoxCollider>();
            //collider.radius = 0.4f;
            collider.isTrigger = true;

            var interaction = gameObject.AddComponent<InteractReceiver>();
            interaction.SetInteractRange(2);
            interaction.SetValue("_usableInShip", true);
            interaction.SetPromptText(text);
            interaction.OnPressInteract += OnToolInteract;
        }

        void OnToolInteract () {
            ControllerInput.SimulateButton(button);
        }
    }
}
