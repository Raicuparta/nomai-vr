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

        static Dictionary<XboxButton, float> _buttons;
        static Dictionary<SingleAxis, float> _singleAxes;
        static Dictionary<DoubleAxis, Vector2> _doubleAxes;

        void Start() {
            NomaiVR.Log("Started SteamVRTest");

            var hand = new GameObject();

            _buttons = new Dictionary<XboxButton, float>();
            _singleAxes = new Dictionary<SingleAxis, float>();
            _doubleAxes = new Dictionary<DoubleAxis, Vector2>();

            SteamVR.Initialize();

            SteamVR_Actions.default_A.onChange += CreateButtonHandler(XboxButton.A);
            SteamVR_Actions.default_B.onChange += CreateButtonHandler(XboxButton.B);
            SteamVR_Actions.default_X.onChange += CreateButtonHandler(XboxButton.X);
            SteamVR_Actions.default_Y.onChange += CreateButtonHandler(XboxButton.Y);
            SteamVR_Actions.default_LB.onChange += CreateButtonHandler(XboxButton.LeftBumper);
            SteamVR_Actions.default_RB.onChange += CreateButtonHandler(XboxButton.RightBumper);
            SteamVR_Actions.default_RT.onChange += CreateSingleAxisHandler(XboxAxis.rightTrigger);
            SteamVR_Actions.default_LT.onChange += CreateSingleAxisHandler(XboxAxis.leftTrigger);
            SteamVR_Actions.default_RStick.onChange += CreateDoubleAxisHandler(XboxAxis.rightStick, XboxAxis.rightStickX, XboxAxis.rightStickY);
            SteamVR_Actions.default_LStick.onChange += CreateDoubleAxisHandler(XboxAxis.leftStick, XboxAxis.leftStickX, XboxAxis.leftStickY);

            NomaiVR.Helper.HarmonyHelper.AddPostfix<SingleAxisCommand>("Update", typeof(Patches), "UpdatePrefix");
        }

        SteamVR_Action_Single.ChangeHandler CreateSingleAxisHandler(SingleAxis singleAxis) {
            return (SteamVR_Action_Single fromAction, SteamVR_Input_Sources fromSource, float newAxis, float newDelta) => {
                _singleAxes[singleAxis] = newAxis;
            };
        }

        SteamVR_Action_Boolean.ChangeHandler CreateButtonHandler(XboxButton button) {
            return (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) => {
                _buttons[button] = newState ? 1 : 0;
            };
        }

        SteamVR_Action_Vector2.ChangeHandler CreateDoubleAxisHandler(DoubleAxis doubleAxis, SingleAxis singleX, SingleAxis singleY) {
            return (SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta) => {
                _doubleAxes[doubleAxis] = axis;
                _singleAxes[singleX] = axis.x;
                _singleAxes[singleY] = axis.y;
            };
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
            //UpdateInputs();
        }

        void UpdateInputs() {
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
            static void UpdatePrefix(
                SingleAxisCommand __instance,
                XboxButton ____xboxButtonPositive,
                XboxButton ____xboxButtonNegative,
                SingleAxis ____gamepadAxisPositive,
                SingleAxis ____gamepadAxisNegative,
                ref float ____value,
                ref bool ____newlyPressedThisFrame,
                ref float ____lastValue,
                ref float ____lastPressedDuration,
                ref float ____pressedDuration,
                ref float ____realtimeSinceLastUpdate,
                int ____axisDirection
            ) {
                ____newlyPressedThisFrame = false;
                ____lastValue = ____value;
                ____value = 0f;

                if (____gamepadAxisPositive != null && _singleAxes.ContainsKey(____gamepadAxisPositive)) {
                    ____value += _singleAxes[____gamepadAxisPositive] * (float)____axisDirection;

                    if (____gamepadAxisNegative != null && _singleAxes.ContainsKey(____gamepadAxisNegative)) {
                        ____value -= _singleAxes[____gamepadAxisNegative] * (float)____axisDirection;
                    }
                } else {
                    if (_buttons.ContainsKey(____xboxButtonPositive)) {
                        ____value += _buttons[____xboxButtonPositive];
                    }

                    if (_buttons.ContainsKey(____xboxButtonNegative)) {
                        ____value -= _buttons[____xboxButtonNegative];
                    }
                }

                //if (OWInput.UsingGamepad()) {
                //    if (this._gamepadAxisPositive != null) {
                //        this._value += this._gamepadAxisPositive.GetValue() * (float)this._axisDirection;
                //        if (this._gamepadAxisNegative != null) {
                //            this._value -= this._gamepadAxisNegative.GetValue() * (float)this._axisDirection;
                //        }
                //    } else {
                //        if (global::Input.GetKey(gamepadKeyCodePositive)) {
                //            this.SetLastPressedJoystick(gamepadKeyCodePositive);
                //            this._value += 1f;
                //        }
                //        if (global::Input.GetKey(gamepadKeyCodeNegative)) {
                //            this.SetLastPressedJoystick(gamepadKeyCodeNegative);
                //            this._value -= 1f;
                //        }
                //    }
                //} else if (this._mouseAxis != null) {
                //    this._value = this._mouseAxis.GetValue() * (float)this._axisDirection;
                //} else {
                //    this._value = ((!global::Input.GetKey(this._keyPositive)) ? this._value : (this._value + 1f));
                //    this._value = ((!global::Input.GetKey(this._keyNegative)) ? this._value : (this._value - 1f));
                //}

                ____lastPressedDuration = ____pressedDuration;
                ____pressedDuration = ((!__instance.IsPressed()) ? 0f : (____pressedDuration + (Time.realtimeSinceStartup - ____realtimeSinceLastUpdate)));
                ____realtimeSinceLastUpdate = Time.realtimeSinceStartup;
            }
        }
    }
}
