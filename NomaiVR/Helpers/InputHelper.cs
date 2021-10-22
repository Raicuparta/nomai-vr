namespace NomaiVR.Helpers
{
    public static class InputHelper
    {
        public static bool IsUIInteractionMode(bool includeDialogue = false)
        {
            return OWInput.IsInputMode(
                InputMode.Menu |
                InputMode.KeyboardInput |
                (includeDialogue ? InputMode.Dialogue : 0)
            );
        }

        public static bool IsStationaryToolMode()
        {
            return OWInput.IsInputMode(
                InputMode.StationaryProbeLauncher |
                InputMode.SatelliteCam
            );
        }

        public static bool IsHandheldTool()
        {
            return OWInput.IsInputMode(
                InputMode.Character |
                InputMode.ScopeZoom
            );
        }
    }
}
