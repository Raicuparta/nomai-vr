using System;
using System.Collections.Generic;

namespace NomaiVR
{
    internal class VRToolSwapper : NomaiVRModule<NomaiVRModule.EmptyBehaviour, VRToolSwapper.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        private static bool _isBuccklingUp = false;
        public static Hand InteractingHand { get; private set; }
        public static Hand NonInteractingHand { get; private set; }

        public static event Action InteractingHandChanged;
        public static event Action ToolEquipped;
        public static event Action Equipped;
        public static event Action UnEquipped;

        private static readonly Dictionary<ToolMode, bool> _toolsAllowedToEquip = new Dictionary<ToolMode, bool>() {
            { ToolMode.Item, true }
        };

        public static void Equip(ToolMode mode, Hand interactingHand)
        {
            _toolsAllowedToEquip[mode] = true;
            ToolHelper.Swapper.EquipToolMode(mode);
            UpdateHand(interactingHand);
            Equipped?.Invoke();
            if (mode != ToolMode.Item && mode != ToolMode.None) ToolEquipped?.Invoke();
            _toolsAllowedToEquip[mode] = false;
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
            if (_isBuccklingUp || (OWInput.IsInputMode(InputMode.ShipCockpit) && mode == ToolMode.None))
            {
                _isBuccklingUp = false;
                return true;
            }
            return _toolsAllowedToEquip.ContainsKey(mode) && _toolsAllowedToEquip[mode];
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
                _isBuccklingUp = true;
            }

            private static bool PreEquipTool(ToolMode mode)
            {
                return IsAllowedToEquip(mode);
            }
        }
    }
}