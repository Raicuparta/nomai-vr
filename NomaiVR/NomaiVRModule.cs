using System.Linq;
using UnityEngine;

namespace NomaiVR
{
    public abstract class NomaiVRModule<Behaviour, Patch>
        where Patch : NomaiVRPatch, new()
        where Behaviour : MonoBehaviour
    {
        protected static OWScene[] PlayableScenes = new[] { OWScene.SolarSystem, OWScene.EyeOfTheUniverse };
        protected static OWScene[] TitleScene = new[] { OWScene.TitleScreen };
        protected static OWScene[] SolarSystemScene = new[] { OWScene.SolarSystem };
        protected static OWScene[] AllScenes = new OWScene[] { };

        protected abstract bool isPersistent { get; }
        protected abstract OWScene[] scenes { get; }

        public NomaiVRModule()
        {
            if (IsSceneRelevant(LoadManager.GetCurrentScene()))
            {
                SetupBehaviour();
            }

            LoadManager.OnCompleteSceneLoad += OnSceneLoad;

            SetupPatch();
        }

        private void OnSceneLoad(OWScene originalScene, OWScene loadScene)
        {
            if (IsSceneRelevant(loadScene))
            {
                SetupBehaviour();
            }
        }

        private bool IsSceneRelevant(OWScene scene)
        {
            return scenes.Length == 0 ? true : scenes.Contains(scene);
        }

        private void SetupBehaviour()
        {
            if (typeof(Behaviour) == typeof(NomaiVRModule.EmptyBehaviour))
            {
                return;
            }

            NomaiVR.Log("Creating NomaiVR behaviour for", GetType().Name);
            var gameObject = new GameObject();
            gameObject.AddComponent<Behaviour>();

            if (isPersistent)
            {
                gameObject.AddComponent<PersistObject>();
            }
        }

        private void SetupPatch()
        {
            if (typeof(Patch) == typeof(NomaiVRModule.EmptyPatch))
            {
                return;
            }

            NomaiVR.Log("Applying NomaiVR patches for", GetType().Name);
            var patch = new Patch();
            patch.ApplyPatches();
        }
    }

    public static class NomaiVRModule
    {
        public class EmptyPatch : NomaiVRPatch
        {
            public override void ApplyPatches()
            { }
        }

        public class EmptyBehaviour : MonoBehaviour
        { }
    }
}
