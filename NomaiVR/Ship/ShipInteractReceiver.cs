using UnityEngine;

namespace NomaiVR.Ship
{
    public abstract class ShipInteractReceiver : MonoBehaviour
    {
        protected abstract UITextType Text { get; }
        protected abstract string ChildName { get;  }
        protected static readonly Font MonitorPromptFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular");
        private InteractReceiver receiver;
        private BoxCollider collider;
        
        private void Awake()
        {
            Initialize();

            var child = string.IsNullOrEmpty(ChildName) ? gameObject : transform.Find(ChildName).gameObject;
            
            collider = child.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.enabled = false;

            receiver = child.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(Text);
            receiver.OnPressInteract += OnPress;
            receiver.OnReleaseInteract += () =>
            {
                OnRelease();
                receiver.ResetInteraction();
            };
        }

        private void Update()
        {
            var isInShip = OWInput.IsInputMode(InputMode.ShipCockpit);
            var isUsingTool = IsUsingTool();
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

        protected abstract void Initialize();
        protected abstract void OnPress();
        protected abstract void OnRelease();
        protected abstract bool IsUsingTool();
        
        protected static bool ShouldRenderScreenText()
        {
            return Locator.GetToolModeSwapper().IsInToolMode(ToolMode.None);
        }
    }
}
