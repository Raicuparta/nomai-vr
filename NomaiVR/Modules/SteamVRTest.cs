using OWML.Common;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using OWML.ModHelper.Events;

namespace NomaiVR
{
    class SteamVRTest : MonoBehaviour
    {
        static bool pressingA = false;

        void Start() {
            NomaiVR.Log("Started SteamVRTest");

            var hand = new GameObject();

            SteamVR.Initialize();

            SteamVR_Actions.default_A.onChange += Default_A_onChange; ;
            SteamVR_Actions.default_B.onChange += OnChangeBoolean;
            SteamVR_Actions.default_X.onChange += OnChangeBoolean;
            SteamVR_Actions.default_Y.onChange += OnChangeBoolean;
            SteamVR_Actions.default_LB.onChange += OnChangeBoolean;
            SteamVR_Actions.default_RB.onChange += OnChangeBoolean;
            SteamVR_Actions.default_Select.onChange += OnChangeBoolean;
            SteamVR_Actions.default_RT.onChange += OnChangeSingle;
            SteamVR_Actions.default_LT.onChange += OnChangeSingle;
            SteamVR_Actions.default_RStick.onChange += OnVector2Change;
            SteamVR_Actions.default_LStick.onChange += OnVector2Change;

            NomaiVR.Helper.HarmonyHelper.AddPostfix<OWInput>("IsPressed", typeof(Patches), "IsPressed");
        }

        private void Default_A_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            //NomaiVR.Log("A " + fromAction.localizedOriginName + " " + newState);
            pressingA = newState;
        }

        private void OnVector2Change(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            //NomaiVR.Log("Vector2 " + fromAction.localizedOriginName + ": " + axis + " - " + delta);
        }

        private void OnChangeSingle(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) {
            //NomaiVR.Log("Single " + fromAction.localizedOriginName + ": " + newAxis + " - " + newDelta);
        }

        private void OnChangeBoolean(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            //NomaiVR.Log("Boolean " + fromAction.localizedOriginName);
        }

        internal static class Patches
        {
            static bool IsPressed(bool __result, SingleAxisCommand command, InputMode mask = InputMode.All) {
                if (command == InputLibrary.jump && SteamVRTest.pressingA) {
                    NomaiVR.Log("jump");
                    //return OWInput.IsInputMode(mask);
                    return true;
                }
                return false;
                //return __result;
            }
        }
    }
}
