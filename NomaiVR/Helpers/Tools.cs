using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NomaiVR
{
    public static class Tools
    {
        public static ToolModeSwapper Swapper => Locator.GetToolModeSwapper();

        public static bool IsUsingAnyTool()
        {
            return Swapper.IsInToolMode(ToolMode.Probe) || Swapper.IsInToolMode(ToolMode.Translator) || Swapper.IsInToolMode(ToolMode.SignalScope);
        }
    }
}
