using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NomaiVR
{
    public static class SceneHelper
    {
        public static bool IsInGame()
        {
            var scene = LoadManager.GetCurrentScene();
            return scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse;
        }
    }
}
