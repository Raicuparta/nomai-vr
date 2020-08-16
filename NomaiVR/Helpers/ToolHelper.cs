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
            return Swapper.IsInToolMode(ToolMode.None) || Swapper.IsInToolMode(ToolMode.Item);
        }
    }
}
