using NomaiVR.Helpers;
using NomaiVR.Input;
using static InputConsts;

namespace NomaiVR.Ship
{
    public class ShipProbeButton : GlowyButton
    {
        private InteractReceiver receiver;
        private UITextType promptText;
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
            return ToolHelper.IsInToolMode(ToolMode.Probe, ToolGroup.Ship);
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
            receiver.SetPromptText(promptText);
            receiver.OnPressInteract += OnPressInteract;
            receiver.OnReleaseInteract += OnReleaseInteract;
            receiver.OnLoseFocus += OnReleaseInteract;
        }

        private void SetUpButtonAction()
        {
            switch (name)
            {
                case "Up":
                    inputCommandType = InputCommandType.TOOL_UP;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Down":
                    inputCommandType = InputCommandType.TOOL_DOWN;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Left":
                    inputCommandType = InputCommandType.TOOL_LEFT;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Right":
                    inputCommandType = InputCommandType.TOOL_RIGHT;
                    promptText = UITextType.ProbeRotatePrompt;
                    break;
                case "Shoot":
                    inputCommandType = InputCommandType.TOOL_PRIMARY;
                    break;
                case "Retrieve":
                    inputCommandType = InputCommandType.PROBERETRIEVE;
                    promptText = UITextType.ProbeRetrievePrompt;
                    break;
            }
        }
    }
}