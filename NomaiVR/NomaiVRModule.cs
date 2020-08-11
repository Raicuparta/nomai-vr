using System.Linq;
using UnityEngine;

namespace NomaiVR
{
    internal abstract class NomaiVRModule<Behaviour, Patch>
        where Patch : NomaiVRPatch, new()
        where Behaviour : MonoBehaviour
    {
        protected static OWScene[] PlayableScenes = new[] { OWScene.SolarSystem, OWScene.EyeOfTheUniverse };
        protected static OWScene[] TitleScene = new[] { OWScene.TitleScreen };
        protected static OWScene[] SolarSystemScene = new[] { OWScene.SolarSystem };
        protected static OWScene[] AllScenes = new OWScene[] { };

        protected abstract bool IsPersistent { get; }
        protected abstract OWScene[] Scenes { get; }

        private bool _isPersistentBehaviourSetUp;

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
            return Scenes.Length == 0 || Scenes.Contains(scene);
        }

        private void SetupBehaviour()
        {
            if (_isPersistentBehaviourSetUp || typeof(Behaviour) == typeof(NomaiVRModule.EmptyBehaviour))
            {
                return;
            }

            Logs.WriteInfo($"Creating NomaiVR behaviour for {GetType().Name}");
            var gameObject = new GameObject();
            gameObject.AddComponent<Behaviour>();

            if (IsPersistent)
            {
                gameObject.AddComponent<PersistObject>();
                _isPersistentBehaviourSetUp = true;
            }
        }

        private void SetupPatch()
        {
            if (typeof(Patch) == typeof(NomaiVRModule.EmptyPatch))
            {
                return;
            }

            Logs.WriteInfo($"Applying NomaiVR patches for {GetType().Name}");
            var patch = new Patch();
            patch.ApplyPatches();
        }
    }

    internal static class NomaiVRModule
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
