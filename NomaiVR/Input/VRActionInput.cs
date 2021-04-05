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
        private readonly bool _isDynamic;
        private readonly HashSet<string> _prefixes = new HashSet<string>();
        private readonly ISteamVR_Action_In _action;
        private readonly VRActionInput _holdActionInput;

        private SteamVR_Input_Sources? _dynamicSource;
        public bool Dynamic => _isDynamic || (_holdActionInput != null && _holdActionInput.Dynamic);
        public bool Active => _isDynamic ? (_dynamicSource != null && _action.GetActive(_dynamicSource.Value)) : _action.active;

        public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false, VRActionInput holdActionInput = null, bool isDynamic = false)
        {
            _color = color;
            _isDynamic = isDynamic;
            _action = action;
            _holdActionInput = holdActionInput;

            if (isLongPress)
            {
                _prefixes.Add("Long Press");
            }
        }

        public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false, VRActionInput holdActionInput = null, bool isDynamic = false) : this(action, TextHelper.ORANGE, isLongPress, holdActionInput, isDynamic) { }

        public VRActionInput(ISteamVR_Action_In action, VRActionInput holdActionInput) : this(action, TextHelper.ORANGE, false, holdActionInput) { }

        public void BindSource(SteamVR_Input_Sources inputSource)
        {
            if (_isDynamic)
                _dynamicSource = inputSource;
        }

        public void Initialize()
        {
            _hand = Active ? _action.GetLocalizedOriginPart(_dynamicSource ?? _action.activeDevice, new[] { EVRInputStringBits.VRInputString_Hand }) : "";
            _source = Active ? _action.GetLocalizedOriginPart(_dynamicSource ?? _action.activeDevice, new[] { EVRInputStringBits.VRInputString_InputSource }) : "";

            if (_holdActionInput  != null && _holdActionInput.Dynamic)
                _holdActionInput.Initialize();

            if (!_isDynamic && string.IsNullOrEmpty(_hand) && string.IsNullOrEmpty(_source))
            {
                Logs.WriteError($"Could not find name for binding {_action.GetShortName()}.");
            }
        }

        public string[] GetText()
        {
            var prefix = GetPrefixText();
            var result = $"{prefix}{GetColoredLocalizedText()}";
            if (string.IsNullOrEmpty(result))
            {
                return new string[] { };
            }
            return GetHoldActionText().Concat(new[] { WrapWithBrackets(result) }).ToArray();
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

        public bool DependsOnActionSet(SteamVR_ActionSet actionSet)
        {
            return _action.actionSet == actionSet || (_holdActionInput != null && _holdActionInput.DependsOnActionSet(actionSet));
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
                return new string[0];
            }
            return _holdActionInput.GetText();
        }
    }
}
