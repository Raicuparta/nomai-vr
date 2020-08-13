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

        public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false)
        {
            _color = color;
            _action = action;

            if (isLongPress)
            {
                _prefixes.Add("Long Press");
            }
        }

        public void Initialize()
        {
            _hand = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_Hand });
            _source = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_InputSource });

            // TODO: need to find a better way to detect this
            //if (string.IsNullOrEmpty(_hand) && string.IsNullOrEmpty(_source))
            //{
            //    Logs.WriteError($"Could not find name for binding {_action.GetShortName()}.");
            //    FatalErrorChecker.ThrowSteamVRError();
            //}
        }

        public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false) : this(action, TextHelper.ORANGE, isLongPress) { }

        public string GetText()
        {
            ControllerInput.Behaviour.InitializeActionInputs();
            var prefix = _prefixes.Count > 0 ? $"{TextHelper.TextWithColor(string.Join(" ", _prefixes.ToArray()), TextHelper.ORANGE)} " : "";
            var hand = HideHand ? "" : $"{_hand} ";
            var result = $"{prefix}{TextHelper.TextWithColor($"{hand}{_source}", _color)}";
            return string.IsNullOrEmpty(result) ? "" : $"[{result}]";
        }

        public bool IsOppositeHandWithSameName(VRActionInput other)
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
    }
}
