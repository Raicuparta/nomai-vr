namespace NomaiVR
{
    public static class SceneHelper
    {
        public static bool IsInGame()
        {
            var scene = LoadManager.GetCurrentScene();
            return scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse;
        }
        public static bool IsInTitle()
        {
            return LoadManager.GetCurrentScene() == OWScene.TitleScreen;
        }
    }
}
