using UnityEngine;

namespace NomaiVR.Ship
{
    public abstract class ShipInteractReceiver : MonoBehaviour
    {
        protected abstract UITextType Text { get; }
        protected abstract GameObject ComponentContainer { get; }
        protected static readonly Font MonitorPromptFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular");
        protected InteractReceiver Receiver { get; private set; }
        private BoxCollider collider;
        
        protected virtual void Awake()
        {
            collider = ComponentContainer.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.enabled = false;

            Receiver = ComponentContainer.AddComponent<InteractReceiver>();
            Receiver.SetInteractRange(2);
            Receiver._usableInShip = true;
            Receiver.SetPromptText(Text);
            Receiver.OnPressInteract += OnPress;
            Receiver.OnReleaseInteract += () =>
            {
                OnRelease();
                Receiver.ResetInteraction();
            };
        }

        protected virtual void Update()
        {
            var isInShip = OWInput.IsInputMode(InputMode.ShipCockpit);
            var shouldDisable = ShouldDisable();
            if (!collider.enabled && isInShip && !shouldDisable)
            {
                collider.enabled = true;
            }
            if (collider.enabled && (!isInShip || shouldDisable))
            {
                collider.enabled = false;
            }
        }

        protected abstract void OnPress();
        protected abstract void OnRelease();
        protected abstract bool ShouldDisable();
        
        protected static bool ShouldRenderScreenText()
        {
            return Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None);
        }
    }
}
