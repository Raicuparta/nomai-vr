using NomaiVR.Helpers;
using NomaiVR.Input;
using static InputConsts;

namespace NomaiVR.Ship
{
    public class ShipSignalscopeButton : GlowyButton
    {
        private InteractReceiver receiver;
        private InputCommandType inputCommandType;

        protected override void Initialize()
        {
            SetUpButtonAction();
            SetUpReceiver();
        }

        protected override bool IsButtonActive()
        {
            return receiver._isInteractPressed;
        }

        protected override bool IsButtonFocused()
        {
            var isFocused = receiver.IsFocused();
            if (isFocused)
            {
                UpdatePrimaryPrompt();
            }

            return isFocused;
        }

        protected override bool IsButtonEnabled()
        {
            return ToolHelper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Ship);
        }

        private void OnDestroy()
        {
            CleanupReceiverEvents();
        }

        private void CleanupReceiverEvents()
        {            
            receiver.OnPressInteract -= OnPressInteract;
            receiver.OnReleaseInteract -= OnReleaseInteract;
            receiver.OnLoseFocus -= OnReleaseInteract;
        }

        private void OnPressInteract()
        {
            ControllerInput.SimulateInput(inputCommandType, true);
        }

        private void OnReleaseInteract()
        {
            ControllerInput.SimulateInput(inputCommandType, false);
            receiver.ResetInteraction();
        }

        private void UpdatePrimaryPrompt()
        {
            if (name != "Shoot") return;
            var isProbeLaunched = Locator.GetProbe() && Locator.GetProbe().IsLaunched();
            receiver.SetPromptText(isProbeLaunched
                ? UITextType.ProbeSnapshotPrompt
                : UITextType.ProbeLaunchPrompt);
        }

        private void SetUpReceiver()
        {
            receiver = gameObject.AddComponent<InteractReceiver>();
            receiver.SetInteractRange(2);
            receiver._usableInShip = true;
            receiver.SetPromptText(UITextType.SignalscopeFrequencyPrompt);
            receiver.OnPressInteract += OnPressInteract;
            receiver.OnReleaseInteract += OnReleaseInteract;
            receiver.OnLoseFocus += OnReleaseInteract;
        }

        private void SetUpButtonAction()
        {
            switch (name)
            {
                case "Left":
                    inputCommandType = InputCommandType.TOOL_LEFT;
                    break;
                case "Right":
                    inputCommandType = InputCommandType.TOOL_RIGHT;
                    break;
            }
        }
    }
}