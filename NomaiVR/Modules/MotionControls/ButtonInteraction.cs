using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR {
    class ButtonInteraction: MonoBehaviour {
        public XboxButton button;

        void Awake () {
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.4f;
            collider.isTrigger = true;

            var interaction = gameObject.AddComponent<InteractReceiver>();
            interaction.SetInteractRange(2);
            interaction.SetValue("_usableInShip", true);
            interaction.SetPromptText(UITextType.Probe_Title);
            interaction.OnPressInteract += OnToolInteract;
        }

        void OnToolInteract () {
            ControllerInput.SimulateButton(button);
        }
    }
}
