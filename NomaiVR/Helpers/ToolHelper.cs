namespace NomaiVR
{
    public static class ToolHelper
    {
        public static ToolModeSwapper Swapper => Locator.GetToolModeSwapper();

        public static bool IsUsingAnyTool(ToolGroup group = ToolGroup.None)
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
