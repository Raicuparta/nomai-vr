using System;
using System.Collections.Generic;

namespace NomaiVR
{
    internal class VRToolSwapper : NomaiVRModule<NomaiVRModule.EmptyBehaviour, VRToolSwapper.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        private static bool _isBuccklingUp = false;
        private static Hand _pendingHandInteract;
        public static Hand InteractingHand { get; private set; }
        public static Hand NonInteractingHand { get; private set; }

        public static event Action InteractingHandChanged;
        public static event Action Equipped;
        public static event Action UnEquipped;

        private static readonly Dictionary<ToolMode, bool> _toolsAllowedToEquip = new Dictionary<ToolMode, bool>() {
            { ToolMode.Item, true }
        };

        public static void Equip(ToolMode mode, Hand interactingHand)
        {
            _toolsAllowedToEquip[mode] = true;
            _pendingHandInteract = interactingHand;
            ToolHelper.Swapper.EquipToolMode(mode);
            _toolsAllowedToEquip[mode] = false;
        }

        public static void Unequip()
        {
            _toolsAllowedToEquip[ToolMode.None] = true;
            ToolHelper.Swapper.UnequipTool();
            _toolsAllowedToEquip[ToolMode.None] = false;
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

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<ToolModeSwapper>(nameof(ToolModeSwapper.EquipToolMode), nameof(PreEquipTool));
                Postfix<ToolModeSwapper>(nameof(ToolModeSwapper.EquipToolMode), nameof(PostEquipTool));
                Prefix<ToolModeSwapper>(nameof(ToolModeSwapper.UnequipTool), nameof(PreUnequipTool));
                Prefix<ShipCockpitController>(nameof(ShipCockpitController.OnPressInteract), nameof(PreShipCockpitController));
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

            private static void PreShipCockpitController()
            {
                _isBuccklingUp = true;
            }

            private static bool PreEquipTool(ToolMode mode, ToolModeSwapper __instance, out PlayerTool __state)
            {
                __state = __instance._equippedTool;
                return IsAllowedToEquip(mode);
            }

            private static void PostEquipTool(ToolModeSwapper __instance, PlayerTool __state)
            {
                if(__state != __instance._equippedTool
                    && __instance._equippedTool != null)
                {
                    UpdateHand(_pendingHandInteract);
                    _pendingHandInteract = null;
                    Equipped?.Invoke();
                }
            }

            private static void PreUnequipTool()
            {
                if (IsAllowedToEquip(ToolMode.None))
                {
                    UpdateHand(null);
                    UnEquipped?.Invoke();
                }
            }
        }
    }
}