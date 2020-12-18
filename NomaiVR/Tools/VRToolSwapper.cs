using System.Collections.Generic;

namespace NomaiVR
{
    internal class VRToolSwapper : NomaiVRModule<NomaiVRModule.EmptyBehaviour, VRToolSwapper.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        private static bool _isBuccklingUp = false;

        private static readonly Dictionary<ToolMode, bool> _toolsAllowedToEquip = new Dictionary<ToolMode, bool>() {
            { ToolMode.Item, true }
        };

        public static void Equip(ToolMode mode)
        {
            _toolsAllowedToEquip[mode] = true;
            ToolHelper.Swapper.EquipToolMode(mode);
            _toolsAllowedToEquip[mode] = false;
        }

        public static void Unequip()
        {
            Equip(ToolMode.None);
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