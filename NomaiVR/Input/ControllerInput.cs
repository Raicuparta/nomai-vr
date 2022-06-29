using System.Collections.Generic;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.Tools;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.Input
{
    internal class ControllerInput : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private static readonly Dictionary<int, bool> simulatedBoolInputs = new Dictionary<int, bool>();
        private static readonly List<InputCommandType> inputsToClear = new List<InputCommandType>();

        public static void SimulateInput(InputCommandType commandType, bool value)
        {
            if (value)
            {
                simulatedBoolInputs[(int)commandType] = true;
            }
            else
            {
                simulatedBoolInputs.Remove((int)commandType);
            }
        }
        
        public static void SimulateInput(InputCommandType commandType)
        {
            inputsToClear.Add(commandType);
            simulatedBoolInputs[(int)commandType] = true;
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.UpdateFromAction), nameof(PatchInputCommands));
                Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.UpdateFromAction), nameof(PatchInputCommands));
                Postfix<CompositeInputCommands>(nameof(CompositeInputCommands.UpdateFromAction), nameof(PatchInputCommands));
                Prefix<InputManager>(nameof(InputManager.Rumble), nameof(DoRumble));
                Postfix<InputManager>(nameof(InputManager.IsGamepadEnabled), nameof(ForceGamepadEnabled));
                Postfix<InputManager>(nameof(InputManager.UsingGamepad), nameof(ForceGamepadEnabled));

                Prefix<PlayerCharacterController>(nameof(PlayerCharacterController.UpdateMovement), nameof(PreUpdateMovement));
                Postfix<PlayerCharacterController>(nameof(PlayerCharacterController.UpdateMovement), nameof(PostUpdateMovement));

                Postfix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.HasSameBinding), nameof(PreventSimulatedHasSameBinding));
                Postfix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.HasSameBinding), nameof(PreventSimulatedHasSameBinding));

                VRToolSwapper.ToolEquipped += OnToolEquipped;
                VRToolSwapper.UnEquipped += OnToolUnequipped;
            }

            private static void PreUpdateMovement(PlayerCharacterController __instance, out float __state)
            {
                //Prevents walking when holding tools and lantern
                __state = __instance._walkSpeed;
                if(ShouldPreventWalk)
                    __instance._walkSpeed = __instance._runSpeed;
            }

            private static void PostUpdateMovement(PlayerCharacterController __instance, float __state)
            {
                if (ShouldPreventWalk)
                    __instance._walkSpeed = __state;
            }

            private static bool ShouldPreventWalk => IsToolsetActive || Locator.GetToolModeSwapper()?.GetItemCarryTool()?.GetHeldItemType() == ItemType.DreamLantern;

            private static bool IsToolsetActive => SteamVR_Actions.tools.IsActive(SteamVR_Input_Sources.RightHand)
                                            || SteamVR_Actions.tools.IsActive(SteamVR_Input_Sources.LeftHand);

            private static void OnToolUnequipped()
            {
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.RightHand);
            }

            private static void OnToolEquipped()
            {
                if (VRToolSwapper.InteractingHand != null)
                {
                    SteamVR_Actions.tools.Activate(VRToolSwapper.InteractingHand.isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand, 1);
                }
            }

            private static bool GetSimulatedInput(InputCommandType commandType)
            {
                return simulatedBoolInputs.ContainsKey((int)commandType) && simulatedBoolInputs[(int)commandType];
            }

            private static void ClearSimulatedInputs()
            {
                foreach (var inputToClear in inputsToClear)
                {
                    SimulateInput(inputToClear, false);
                }
                inputsToClear.Clear();
            }
            
            private static void PatchInputCommands(AbstractCommands __instance)
            {
                var commandType = __instance.CommandType;
                if (GetSimulatedInput(commandType))
                {
                    ClearSimulatedInputs();
                    __instance.AxisValue = new Vector2(1f, 0f);
                    return;
                }

                var actionInput = InputMap.GetActionInput(commandType);
                if (actionInput == null) return;
                __instance.AxisValue = actionInput.Value;
            }

            public static void DoRumble(float hiPower, float lowPower)
            {
                if (hiPower <= float.Epsilon && lowPower <= float.Epsilon) return;

                hiPower *= 1.42857146f;
                lowPower *= 1.42857146f;
                var haptic = SteamVR_Actions.default_Haptic;
                var frequency = 0.1f;
                var amplitudeY = lowPower * ModSettings.VibrationStrength;
                var amplitudeX = hiPower * ModSettings.VibrationStrength;
                haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.RightHand);
                haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.RightHand);
                haptic.Execute(0, frequency, 10, amplitudeY, SteamVR_Input_Sources.LeftHand);
                haptic.Execute(0, frequency, 50, amplitudeX, SteamVR_Input_Sources.LeftHand);
            }

            private static void ForceGamepadEnabled(ref bool __result)
            {
                __result = true;
            }
            
            private static void PreventSimulatedHasSameBinding(ref bool __result, IInputCommands __instance, IInputCommands compare)
            {
                if (GetSimulatedInput(__instance.CommandType) || GetSimulatedInput(compare.CommandType))
                {
                    __result = false;
                }
            }
        }
    }
}