using System;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.Tools;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    internal class ShipMonitorInteraction : MonoBehaviour
    {
        public ToolMode mode;
        public InputConsts.InputCommandType button;
        public UITextType text;
        public InteractReceiver receiver;
        public Func<bool> SkipPressCallback;
        private BoxCollider collider;

        internal void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.enabled = false;

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
            if (!collider.enabled && isInShip && !isUsingTool)
            {
                collider.enabled = true;
            }
            if (collider.enabled && (!isInShip || isUsingTool))
            {
                collider.enabled = false;
            }
        }

        public bool IsFocused()
        {
            return receiver && receiver.IsFocused();
        }

        private void OnPress()
        {
            if (SkipPressCallback != null && SkipPressCallback.Invoke()) return;
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
