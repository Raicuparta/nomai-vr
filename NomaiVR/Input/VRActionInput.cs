using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using UnityEngine;

namespace NomaiVR
{
    public class VRActionInput
    {
        public bool HideHand = false;
        public string Hand { get; private set; }
        public string Source { get; private set; }
        public readonly string Color;
        public readonly List<string> Prefixes = new List<string>();
        private bool _isSetupDone = false;
        private ISteamVR_Action_In _action;

        public VRActionInput(ISteamVR_Action_In action, string color, bool isLongPress = false)
        {
            _action = action;
            Color = color;

            if (isLongPress)
            {
                Prefixes.Add("Long Press");
            }

            //TimerHelper.ExecuteRepeating(SetUpInputNames, 1000);
        }

        public VRActionInput(ISteamVR_Action_In action, bool isLongPress = false) : this(action, TextHelper.ORANGE, isLongPress) { }

        public string GetText()
        {
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

        private void SetUpInputNames()
        {
            if (!_action.active || _isSetupDone)
            {
                NomaiVR.Log("attempt failed...", OWML.Common.MessageType.Warning);
                return;
            }

            Hand = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_Hand });
            Source = _action.GetLocalizedOriginPart(SteamVR_Input_Sources.Any, new[] { EVRInputStringBits.VRInputString_InputSource });
            _isSetupDone = true;
            NomaiVR.Log("attempt succeded!", OWML.Common.MessageType.Success);
        }
    }
}
