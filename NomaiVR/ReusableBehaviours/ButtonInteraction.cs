using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ButtonInteraction : MonoBehaviour
    {
        public XboxButton button;
        public UITextType text;
        public InteractReceiver receiver;
        public Func<bool> skipPressCallback;
        private BoxCollider _collider;

        private void Start()
        {
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.isTrigger = true;

            receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver.SetValue("_usableInShip", true);
            receiver.SetPromptText(text);
            receiver.OnPressInteract += OnPress;
            receiver.OnReleaseInteract += OnRelease;
        }

        private void OnPress()
        {
            var skip = skipPressCallback != null && skipPressCallback.Invoke();
            if (!skip)
            {
                ControllerInput.Behaviour.SimulateInput(button, 1);
            }
        }

        private void OnRelease()
        {
            ControllerInput.Behaviour.SimulateInput(button, 0);
            receiver.ResetInteraction();
        }

        private void OnDisable()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }
    }
}
