using System;
using System.Collections.Generic;
using NomaiVR.Hands;
using NomaiVR.Helpers;

namespace NomaiVR.Tools
{
    internal class VRToolSwapper : NomaiVRModule<NomaiVRModule.EmptyBehaviour, VRToolSwapper.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        private static bool isBuccklingUp;
        public static Hand InteractingHand { get; private set; }
        public static Hand NonInteractingHand { get; private set; }

        public static event Action InteractingHandChanged;
        public static event Action ToolEquipped;
        public static event Action Equipped;
        public static event Action UnEquipped;

        private static readonly Dictionary<ToolMode, bool> toolsAllowedToEquip = new Dictionary<ToolMode, bool>() {
            { ToolMode.Item, true }
        };

        public static void Equip(ToolMode mode, Hand interactingHand)
        {
            toolsAllowedToEquip[mode] = true;
            ToolHelper.Swapper.EquipToolMode(mode);
            UpdateHand(interactingHand);
            Equipped?.Invoke();
            if (mode != ToolMode.Item && mode != ToolMode.None && InputHelper.IsHandheldTool()) ToolEquipped?.Invoke();
            toolsAllowedToEquip[mode] = false;
        }

        public static void Unequip()
        {
            UpdateHand(null);
            UnEquipped?.Invoke();
            Equip(ToolMode.None, null);
        }

        private static void UpdateHand(Hand interactingHand)
        {
            if (interactingHand != null)
            {
                InteractingHand = interactingHand;
                NonInteractingHand = HandsController.Behaviour.RightHandBehaviour == interactingHand ? HandsController.Behaviour.LeftHandBehaviour : HandsController.Behaviour.RightHandBehaviour;
            }
            else
            {
                InteractingHand = null;
                NonInteractingHand = null;
            }

            InteractingHandChanged?.Invoke();
        }

        public static bool IsAllowedToEquip(ToolMode mode)
        {
            if (isBuccklingUp || OWInput.IsInputMode(InputMode.ShipCockpit))
            {
                isBuccklingUp = false;
                return true;
            }
            return toolsAllowedToEquip.ContainsKey(mode) && toolsAllowedToEquip[mode];
        }

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<ToolModeSwapper>("EquipToolMode", nameof(PreEquipTool));
                Prefix<ShipCockpitController>("OnPressInteract", nameof(PreShipCockpitController));
            }

            private static void PreShipCockpitController()
            {
                isBuccklingUp = true;
            }

            private static bool PreEquipTool(ToolMode mode)
            {
                return IsAllowedToEquip(mode);
            }
        }
    }
}