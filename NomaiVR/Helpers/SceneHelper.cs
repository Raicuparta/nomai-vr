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
        public static bool IsInSolarSystem()
        {
            return LoadManager.GetCurrentScene() == OWScene.SolarSystem;
        }
        public static bool IsPreviousScene(OWScene scene)
        {
            return LoadManager.GetPreviousScene() == scene;
        }
    }
}
