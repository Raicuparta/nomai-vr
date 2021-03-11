using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace NomaiVR
{
    internal class VirtualKeyboard : NomaiVRModule<VirtualKeyboard.Behaviour, VirtualKeyboard.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;

        protected override OWScene[] Scenes => TitleScene;

        public class Behaviour : MonoBehaviour
        {
            private static InputField _inputField = null;

            internal void Awake()
            {
                SteamVR_Events.System(EVREventType.VREvent_KeyboardCharInput).Listen(OnKeyboard);
                SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
            }

            private static void OpenKeyboard()
            {
                SteamVR.instance.overlay.ShowKeyboard(0, 0, "Description", 256, "", true, 0);
            }

            private void OnKeyboardClosed(VREvent_t evt)
            {
                if (!_inputField || _inputField.text.Length > 0)
                {
                    return;
                }

                OpenKeyboard();
            }

            private void OnKeyboard(VREvent_t evt)
            {
                if (_inputField == null)
                {
                    return;
                }

                Logs.WriteInfo("text: " + _inputField.text);
                if (evt.data.keyboard.cNewInput == "\b") // backspace
                {
                    if (_inputField.text.Length > 0)
                    {
                        _inputField.text = _inputField.text.Substring(0, _inputField.text.Length - 1);
                    }
                }
                else if (evt.data.keyboard.cNewInput == "\x1b") // enter
                {
                    SteamVR.instance.overlay.HideKeyboard();
                }
                else
                {
                    _inputField.text += evt.data.keyboard.cNewInput;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<InputField>("ActivateInputField", nameof(PostActivatePopupInput));
                    Postfix<InputField>("DeactivateInputField", nameof(PostDeactivatePopupInput));
                }

                private static void PostActivatePopupInput(InputField __instance)
                {
                    _inputField = __instance;
                    OpenKeyboard();
                }

                private static void PostDeactivatePopupInput()
                {
                    _inputField = null;
                }
            }
        }
    }
}
