using System.Collections.Generic;
using System.Linq;
using Valve.VR;

namespace NomaiVR
{
    public class VRActionInput
    {
        public bool HideHand = false;
        // TODO readonly
        public string Hand;
        public string Source;
        public readonly string Color;
        public readonly HashSet<string> Prefixes = new HashSet<string>();
        private ISteamVR_Action_In _action;

        public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false)
        {
            Color = color;
            _action = action;

            if (isLongPress)
            {
                Prefixes.Add("Long Press");
            }
        }

        public void Initialize()
        {
            Hand = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_Hand });
            Source = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_InputSource });
        }

        public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false) : this(action, TextHelper.ORANGE, isLongPress) { }

        public string GetText()
        {
            ControllerInput.Behaviour.InitializeActionInputs();
            var prefix = Prefixes.Count > 0 ? $"{TextHelper.TextWithColor(string.Join(" ", Prefixes.ToArray()), TextHelper.ORANGE)} " : "";
            var hand = HideHand ? "" : $"{Hand} ";
            var result = $"{prefix}{TextHelper.TextWithColor($"{hand}{Source}", Color)}";
            return string.IsNullOrEmpty(result) ? "" : $"[{result}]";
        }

        public bool IsOppositeHandWithSameName(VRActionInput other)
        {
            if (other == this)
            {
                return false;
            }
            if (Hand != other.Hand && Source == other.Source)
            {
                return true;
            }
            return false;
        }
    }
}
