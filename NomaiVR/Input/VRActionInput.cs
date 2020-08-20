using Harmony;
using System.Collections.Generic;
using System.Linq;
using Valve.VR;

namespace NomaiVR
{
    public class VRActionInput
    {
        public bool HideHand = false;

        private string _hand;
        private string _source;
        private readonly string _color;
        private readonly HashSet<string> _prefixes = new HashSet<string>();
        private readonly ISteamVR_Action_In _action;
        private readonly VRActionInput _holdActionInput;

        public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false, VRActionInput holdActionInput = null)
        {
            _color = color;
            _action = action;
            _holdActionInput = holdActionInput;

            if (isLongPress)
            {
                _prefixes.Add("Long Press");
            }
        }

        public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false, VRActionInput holdActionInput = null) : this(action, TextHelper.ORANGE, isLongPress, holdActionInput) { }

        public VRActionInput(ISteamVR_Action_In action, VRActionInput holdActionInput) : this(action, TextHelper.ORANGE, false, holdActionInput) { }

        public void Initialize()
        {
            _hand = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_Hand });
            _source = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_InputSource });

            if (string.IsNullOrEmpty(_hand) && string.IsNullOrEmpty(_source))
            {
                Logs.WriteError($"Could not find name for binding {_action.GetShortName()}.");
                // TODO Some buttons might be missing but that doesn't always mean it's wrong (VIVE)
                //FatalErrorChecker.ThrowSteamVRError();
            }
        }

        public string[] GetText()
        {
            ControllerInput.Behaviour.InitializeActionInputs();
            var prefix = GetPrefixText();
            var result = $"{prefix}{GetColoredLocalizedText()}";
            if (string.IsNullOrEmpty(result))
            {
                return new string[] { };
            }
            var holdInputText = GetHoldActionText();
            return holdInputText.Add(WrapWithBrackets(result)).ToArray();
        }

        public bool HasAxisWithSameName()
        {
            foreach (var axisEntry in ControllerInput.axisActions)
            {
                var axis = axisEntry.Value;
                if (_hand == axis._hand && _source == axis._source)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasOppositeHandButtonWithSameName()
        {
            foreach (var buttonEntry in ControllerInput.buttonActions)
            {
                if (IsOppositeHandWithSameName(buttonEntry.Value))
                {
                    return true;
                }
            }
            foreach (var axisEntry in ControllerInput.axisActions)
            {
                if (IsOppositeHandWithSameName(axisEntry.Value))
                {
                    return true;
                }
            }
            foreach (var otherAction in ControllerInput.otherActions)
            {
                if (IsOppositeHandWithSameName(otherAction))
                {
                    return true;
                }
            }
            return false;
        }

        public void SetAsClickable()
        {
            _prefixes.Add("Click");
        }

        private string GetColoredLocalizedText()
        {
            return TextHelper.TextWithColor($"{GetHandText()}{_source}", _color);
        }

        private string GetHandText()
        {
            if (HideHand)
            {
                return "";
            }
            return $"{_hand} ";
        }

        private string GetPrefixText()
        {
            if (_prefixes.Count == 0)
            {
                return "";
            }
            return $"{TextHelper.TextWithColor(string.Join(" ", _prefixes.ToArray()), TextHelper.ORANGE)} ";
        }

        private string WrapWithBrackets(string text)
        {
            return $"[{text}]";
        }

        private bool IsOppositeHandWithSameName(VRActionInput other)
        {
            if (other == this)
            {
                return false;
            }
            if (_hand != other._hand && _source == other._source)
            {
                return true;
            }
            return false;
        }

        private string[] GetHoldActionText()
        {
            if (_holdActionInput == null)
            {
                return new string[] { };
            }
            return _holdActionInput.GetText();
        }
    }
}
