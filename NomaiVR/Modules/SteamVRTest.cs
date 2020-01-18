using OWML.ModHelper.Events;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Harmony;
using System.Reflection.Emit;

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

            NomaiVR.Helper.HarmonyHelper.Transpile<SingleAxisCommand>("Update", typeof(Patches), "TranspileUpdate");
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

        static void SetInputValues(object inputValue, params string[] inputActions) {
            foreach (string action in inputActions) {
                var actionField = typeof(InputLibrary).GetAnyField(action);
                var actionValue = actionField.GetValue(null);

                var isSingleAxis = actionValue.GetType() == typeof(SingleAxisCommand);

                actionValue.SetValue("_value", inputValue);
            }
        }

        void Update() {
            SetInputValues(a, "jump", "select2", "markEntryOnHUD", "boost", "matchVelocity");
            SetInputValues(b, "cancel");
            SetInputValues(x, "select", "confirm2", "interact", "landingCamera");
            SetInputValues(y, "setDefaults", "swapShipLogMode", "sleep", "suitMenu", "signalscope");
            SetInputValues(lb, "cancelRebinding1", "tabL", "probeReverse", "rollMode");
            SetInputValues(rb, "cancelRebinding2", "tabR", "translate", "scopeView", "probeForward", "probeRetrieve");
            SetInputValues(lStick.x, "right", "menuRight", "thrustX");
            SetInputValues(-lStick.x, "left", "menuLeft");
            SetInputValues(lStick.y, "up", "thrustZ");
            SetInputValues(-lStick.y, "down");
            SetInputValues(rStick.x, "tabR2", "yaw");
            SetInputValues(-rStick.x, "tabL2");
            SetInputValues(rStick.y, "scrollLogText", "pitch");
            SetInputValues(rt, "mapZoom", "extendStick", "thrustUp");
            SetInputValues(lt, "thrustDown");
            SetInputValues(lStick, "moveXZ");
            SetInputValues(rStick, "look");
        }

        internal static class Patches
        {
            static IEnumerable<CodeInstruction> TranspileUpdate(IEnumerable<CodeInstruction> instructions) {
                var codes = new List<CodeInstruction>(instructions);
                NomaiVR.Log("opcode: " + codes[8].opcode.ToString());
                NomaiVR.Log("operand: " + codes[8].operand);
                NomaiVR.Log("labels: " + codes[8].labels);

                codes.RemoveRange(7, 3);

                return codes;
            }
        }
    }
}
