using UnityEngine;

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
        public static bool IsInCreditsScene()
        {
            var scene = LoadManager.GetCurrentScene();
            return scene == OWScene.Credits_Fast || scene == OWScene.Credits_Final || scene == OWScene.PostCreditsScene;
        }
        public static bool IsInPostCredits()
        {
            return LoadManager.GetCurrentScene() == OWScene.PostCreditsScene;
        }
        public static void PrintHierarchy(Transform parent, int depth = 0)
        {
            Logs.Write(depth == 0 ? $"Childs of {parent.name}:" 
                                  : parent.name.PadLeft(parent.name.Length + depth, '-'));
            for (int i = 0; i < parent.childCount; i++)
                PrintHierarchy(parent.GetChild(i), depth + 1);
        }
    }
}
