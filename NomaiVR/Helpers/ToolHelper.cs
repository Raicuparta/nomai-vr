namespace NomaiVR
{
    public static class ToolHelper
    {
        public static ToolModeSwapper Swapper => Locator.GetToolModeSwapper();

        public static bool IsUsingAnyTool()
        {
            if (Swapper == null)
            {
                return false;
            }

            return Swapper.IsInToolMode(ToolMode.Probe) || Swapper.IsInToolMode(ToolMode.Translator) || Swapper.IsInToolMode(ToolMode.SignalScope);
        }

        public static bool IsUsingAnyTool(ToolGroup group)
        {
            if (Swapper == null)
            {
                return false;
            }

            return Swapper.IsInToolMode(ToolMode.Probe, group) || Swapper.IsInToolMode(ToolMode.Translator, group) || Swapper.IsInToolMode(ToolMode.SignalScope, group);
        }

        public static bool IsUsingNoTools()
        {
            if (Swapper == null)
            {
                return true;
            }

            return Swapper.IsInToolMode(ToolMode.None) || Swapper.IsInToolMode(ToolMode.Item);
        }

        public static bool HasUsableItem()
        {
            if (Swapper == null)
            {
                return false;
            }

            var item = Swapper.GetItemCarryTool();
            return Swapper.IsInToolMode(ToolMode.Item) && item != null 
                && (item.GetHeldItemType() == ItemType.VisionTorch || item.GetHeldItemType() == ItemType.DreamLantern);
        }
    }
}
