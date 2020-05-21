using OWML.ModHelper.Events;

namespace NomaiVR
{
    public static class ToolHelper
    {
        public static ToolModeSwapper Swapper => Locator.GetToolModeSwapper();

        public static bool IsUsingAnyTool()
        {
            return Swapper.IsInToolMode(ToolMode.Probe) || Swapper.IsInToolMode(ToolMode.Translator) || Swapper.IsInToolMode(ToolMode.SignalScope);
        }
    }
}
