using OWML.ModHelper.Events;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    class SteamVRTest : MonoBehaviour
    {
        static float a;
        static float b;
        static float x;
        static float y;
        static float lb;
        static float rb;
        static float rt;
        static float lt;
        static Vector2 lStick;
        static Vector2 rStick;

        void Start() {
            NomaiVR.Log("Started SteamVRTest");

            var hand = new GameObject();

            SteamVR.Initialize();

            SteamVR_Actions.default_A.onChange += Default_A_onChange;
            SteamVR_Actions.default_B.onChange += Default_B_onChange;
            SteamVR_Actions.default_X.onChange += Default_X_onChange;
            SteamVR_Actions.default_Y.onChange += Default_Y_onChange;
            SteamVR_Actions.default_LB.onChange += Default_LB_onChange;
            SteamVR_Actions.default_RB.onChange += Default_RB_onChange;
            SteamVR_Actions.default_RT.onChange += Default_RT_onChange;
            SteamVR_Actions.default_LT.onChange += Default_LT_onChange;
            SteamVR_Actions.default_RStick.onChange += Default_RStick_onChange;
            SteamVR_Actions.default_LStick.onChange += Default_LStick_onChange;

            var cona = new {
                pressingA = false,
            };
        }

        void Default_LT_onChange(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) {
            lt = newAxis;
        }

        void Default_RT_onChange(SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) {
            rt = newAxis;
        }

        void Default_RB_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            rb = newState ? 1 : 0;
        }

        void Default_LB_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            lb = newState ? 1 : 0;
        }

        void Default_Y_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            y = newState ? 1 : 0;
        }

        void Default_X_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            x = newState ? 1 : 0;
        }

        void Default_B_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            b = newState ? 1 : 0;
        }

        void Default_A_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            a = newState ? 1 : 0;
        }

        void Default_LStick_onChange(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            lStick = axis;
        }

        void Default_RStick_onChange(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) {
            rStick = axis;
        }

        void SetInputValues(object inputValue, params string[] inputActions) {
            foreach (string action in inputActions) {
                typeof(InputLibrary).GetAnyField(action).GetValue(null).SetValue("_value", inputValue);
            }
        }

        void Update() {
            SetInputValues(a, "jump");
            InputLibrary.moveXZ.SetValue("_value", lStick);
            InputLibrary.look.SetValue("_value", rStick);
            InputLibrary.interact.SetValue("_value", x);
        }
    }
}
