using System.Collections.Generic;
using Valve.VR;

namespace NomaiVR
{
    public class VRActionInput
    {
        public string Hand;
        public readonly string Source;
        public readonly string Color;
        public readonly List<string> Prefixes = new List<string>();

        public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false)
        {
            Hand = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_Hand });
            Source = action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_InputSource });
            Color = color;

            if (isLongPress)
            {
                Prefixes.Add("Long Press");
            }
        }

        public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false) : this(action, TextHelper.ORANGE, isLongPress) { }

        public string GetText()
        {
            var prefix = TextHelper.TextWithColor(string.Join(" ", Prefixes.ToArray()), TextHelper.ORANGE);
            return $"{prefix} {TextHelper.TextWithColor($"{Hand} {Source}", Color)}";
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
