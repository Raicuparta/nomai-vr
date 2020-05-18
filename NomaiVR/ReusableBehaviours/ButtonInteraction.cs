using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR
{
    class ButtonInteraction : MonoBehaviour
    {
        public XboxButton button;
        public UITextType text;
        public InteractReceiver receiver;
        public Func<bool> skipPressCallback;
        BoxCollider _collider;

        void Start()
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

        void OnPress()
        {
            var skip = skipPressCallback != null && skipPressCallback.Invoke();
            if (!skip)
            {
                ControllerInput.Behaviour.SimulateInput(button, 1);
            }
        }

        void OnRelease()
        {
            ControllerInput.Behaviour.SimulateInput(button, 0);
            receiver.ResetInteraction();
        }

        void OnDisable()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        void OnEnable()
        {
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }
    }
}
