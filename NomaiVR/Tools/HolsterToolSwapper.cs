using System.Collections.Generic;

namespace NomaiVR
{
    internal class HolsterToolSwapper : NomaiVRModule<NomaiVRModule.EmptyBehaviour, HolsterToolSwapper.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

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

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Prefix<ToolModeSwapper>("EquipToolMode", nameof(PreEquipTool));
            }

            private static bool PreEquipTool(ToolMode mode)
            {
                return _toolsAllowedToEquip.ContainsKey(mode) && _toolsAllowedToEquip[mode];
            }
        }
    }
}