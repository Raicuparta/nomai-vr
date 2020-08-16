using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR
{
    internal class ShipMonitorInteraction : MonoBehaviour
    {
        public ToolMode mode;
        public JoystickButton button;
        public UITextType text;
        public InteractReceiver receiver;
        public Func<bool> skipPressCallback;
        private BoxCollider _collider;

        internal void Start()
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
                if (mode != ToolMode.None)
                {
                    VRToolSwapper.Equip(mode);
                }
                if (button != JoystickButton.None)
                {
                    ControllerInput.Behaviour.SimulateInput(button, 1);
                }
            }
        }

        private void OnRelease()
        {
            if (button != JoystickButton.None)
            {
                ControllerInput.Behaviour.SimulateInput(button, 0);
            }
            receiver.ResetInteraction();
        }

        internal void OnDisable()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        internal void OnEnable()
        {
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }
    }
}
