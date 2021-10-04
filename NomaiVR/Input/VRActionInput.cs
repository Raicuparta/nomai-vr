// using System.Collections.Generic;
// using System.Linq;
// using Valve.VR;
//
// namespace NomaiVR
// {
//     public class VRActionInput
//     {
//         public bool HideHand { get; set; } = false;
//
//         private readonly string _color;
//         private readonly bool _isDynamic;
//         private readonly HashSet<string> _prefixes = new HashSet<string>();
//         private readonly ISteamVR_Action_In _action;
//         private readonly VRActionInput _holdActionInput;
//         private SteamVR_Input_Sources? _dynamicSource;
//
//         public string HandText { get; private set; }
//         public SteamVR_Input_Sources Hand { get; private set; }
//         public string Source { get; private set; }
//         public bool Dynamic => _isDynamic || (_holdActionInput != null && _holdActionInput.Dynamic);
//         public bool Active => _isDynamic ? (_dynamicSource != null && _action.GetActive(_dynamicSource.Value)) : _action.active;
//
//         public VRActionInput(ISteamVR_Action_In action, string color = TextHelper.BLUE, bool isLongPress = false, bool isClickable = false, VRActionInput holdActionInput = null, bool isDynamic = false)
//         {
//             _color = color;
//             _isDynamic = isDynamic;
//             _action = action;
//             _holdActionInput = holdActionInput;
//
//             if (isLongPress) _prefixes.Add("Long Press");
//             if (isClickable) _prefixes.Add("Click");
//         }
//
//         public void BindSource(SteamVR_Input_Sources inputSource)
//         {
//             if (_isDynamic)
//                 _dynamicSource = inputSource;
//         }
//
//         public void Initialize()
//         {
//             Hand = _action.activeDevice;
//             HandText = Active ? _action.GetLocalizedOriginPart(_dynamicSource ?? _action.activeDevice, new[] { EVRInputStringBits.VRInputString_Hand }) : HandText;
//             Source = Active ? _action.GetLocalizedOriginPart(_dynamicSource ?? _action.activeDevice, new[] { EVRInputStringBits.VRInputString_InputSource }) : Source;
//
//             if (_holdActionInput  != null && _holdActionInput.Dynamic)
//                 _holdActionInput.Initialize();
//         }
//
//         public string[] GetText()
//         {
//             var prefix = GetPrefixText();
//             var result = $"{prefix}{GetColoredLocalizedText()}";
//             if (string.IsNullOrEmpty(result))
//             {
//                 return new string[] { };
//             }
//             return GetHoldActionText().Concat(new[] { WrapWithBrackets(result) }).ToArray();
//         }
//
//         public bool DependsOnActionSet(SteamVR_ActionSet actionSet)
//         {
//             return _action.actionSet == actionSet || (_holdActionInput != null && _holdActionInput.DependsOnActionSet(actionSet));
//         }
//
//         private string GetColoredLocalizedText()
//         {
//             return TextHelper.TextWithColor($"{GetHandText()}{Source}", _color);
//         }
//
//         private string GetHandText()
//         {
//             if (HideHand)
//             {
//                 return "";
//             }
//             return $"{HandText} ";
//         }
//
//         private string GetPrefixText()
//         {
//             if (_prefixes.Count == 0)
//             {
//                 return "";
//             }
//             return $"{TextHelper.TextWithColor(string.Join(" ", _prefixes.ToArray()), TextHelper.ORANGE)} ";
//         }
//
//         private string WrapWithBrackets(string text)
//         {
//             return $"[{text}]";
//         }
//
//         private string[] GetHoldActionText()
//         {
//             if (_holdActionInput == null)
//             {
//                 return new string[0];
//             }
//             return _holdActionInput.GetText();
//         }
//     }
// }
