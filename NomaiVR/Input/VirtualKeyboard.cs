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
                SteamVR_Events.System(EVREventType.VREvent_KeyboardClosed).Listen(OnKeyboardClosed);
            }

            private static void OpenKeyboard()
            {
                SteamVR.instance.overlay.ShowKeyboard(0, 0, 0, "Input Text", 256, _inputField.text, 1);
            }

            private void OnKeyboardClosed(VREvent_t evt)
            {
                StringBuilder text = new StringBuilder(256);
                _inputField.caretPosition = (int)SteamVR.instance.overlay.GetKeyboardText(text, 256);
                _inputField.text = text.ToString();
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<InputField>(nameof(InputField.ActivateInputField), nameof(PostActivatePopupInput));
                    Postfix<InputField>(nameof(InputField.DeactivateInputField), nameof(PostDeactivatePopupInput));
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
