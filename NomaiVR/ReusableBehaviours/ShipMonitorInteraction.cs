
using System;
using NomaiVR.Input;
using UnityEngine;

namespace NomaiVR
{
    internal class ShipMonitorInteraction : MonoBehaviour
    {
        public ToolMode mode;
        public InputConsts.InputCommandType button;
        public UITextType text;
        public InteractReceiver receiver;
        public Func<bool> skipPressCallback;
        private BoxCollider _collider;

        internal void Start()
        {
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.isTrigger = true;
            _collider.enabled = false;

            receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(text);
            receiver.OnPressInteract += OnPress;
            receiver.OnReleaseInteract += OnRelease;
        }

        internal void Update()
        {
            var isInShip = OWInput.IsInputMode(InputMode.ShipCockpit);
            var isUsingTool = mode != ToolMode.None && ToolHelper.Swapper.IsInToolMode(mode, ToolGroup.Ship);
            if (!_collider.enabled && isInShip && !isUsingTool)
            {
                _collider.enabled = true;
            }
            if (_collider.enabled && (!isInShip || isUsingTool))
            {
                _collider.enabled = false;
            }
        }

        public bool IsFocused()
        {
            return receiver && receiver.IsFocused();
        }

        private void OnPress()
        {
            if (skipPressCallback != null && skipPressCallback.Invoke()) return;
            if (mode != ToolMode.None)
            {
                VRToolSwapper.Equip(mode, null);
            }
            if (button != InputConsts.InputCommandType.UNDEFINED)
            {
                ControllerInput.SimulateInput(button, true);
            }
        }

        private void OnRelease()
        {
            if (button != InputConsts.InputCommandType.UNDEFINED)
            {
                ControllerInput.SimulateInput(button, false);
            }
            receiver.ResetInteraction();
        }
    }
}
