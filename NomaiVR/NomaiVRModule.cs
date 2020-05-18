using UnityEngine;
using System.Linq;
using System;

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

        private bool IsSceneRelevant(OWScene scene)
        {
            return scenes.Length == 0 ? true : scenes.Contains(scene);
        }

        private void OnSceneLoad(OWScene originalScene, OWScene loadScene)
        {
            if (IsSceneRelevant(loadScene))
            {
                SetupBehaviour();
                OnSceneLoad();
            }
        }

        protected virtual void OnSceneLoad() { }

        private void SetupBehaviour()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Behaviour>();
            if (isPersistent)
            {
                gameObject.AddComponent<PersistObject>();
            }
        }

        private void SetupPatch()
        {
            var patch = new Patch();
            patch.ApplyPatches();
        }
    }

    public class NomaiVRModule
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
