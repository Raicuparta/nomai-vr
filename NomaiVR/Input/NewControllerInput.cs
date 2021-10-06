using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.Input
{
    internal class NewControllerInput : NomaiVRModule<NomaiVRModule.EmptyBehaviour, NewControllerInput.Patch>
    {
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => TitleScene;

        private static readonly Dictionary<int, bool> simulatedBoolInputs = new Dictionary<int, bool>();

        public static void SimulateInput(InputCommandType commandType, bool value)
        {
            if (!value)
            {
                // TODO maybe not good to keep adding and removing from dictionary.
                simulatedBoolInputs.Remove((int) commandType);
            } 
            simulatedBoolInputs[(int)commandType] = value;
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

                VRToolSwapper.ToolEquipped += OnToolEquipped;
                VRToolSwapper.UnEquipped += OnToolUnequipped;
            }

            private void OnToolUnequipped()
            {
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.LeftHand);
                SteamVR_Actions.tools.Deactivate(SteamVR_Input_Sources.RightHand);
            }

            private void OnToolEquipped()
            {
                if (VRToolSwapper.InteractingHand != null)
                {
                    SteamVR_Actions.tools.Activate(VRToolSwapper.InteractingHand.isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand, 1);
                }
            }

            private static void PatchInputCommands(AbstractCommands __instance)
            {
                var commandType = __instance.CommandType;
                var commandTypeKey = (int)commandType;
                if (simulatedBoolInputs.ContainsKey(commandTypeKey) && simulatedBoolInputs[commandTypeKey])
                {
                    __instance.AxisValue = new Vector2(1f, 0f);
                    return;
                }

                var actionInput = InputMap.GetActionInput(commandType);
                if (actionInput == null) return;
                __instance.AxisValue = actionInput.Value;
            }

            public static void DoRumble(float hiPower, float lowPower)
            {
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
        }
    }
}