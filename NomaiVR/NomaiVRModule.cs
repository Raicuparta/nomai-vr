using System.Linq;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR
{
    internal abstract class NomaiVRModule<TBehaviour, TPatch>
        where TPatch : NomaiVRPatch, new()
        where TBehaviour : MonoBehaviour
    {
        protected static OWScene[] PlayableScenes = new[] { OWScene.SolarSystem, OWScene.EyeOfTheUniverse };
        protected static OWScene[] TitleScene = new[] { OWScene.TitleScreen };
        protected static OWScene[] SolarSystemScene = new[] { OWScene.SolarSystem };
        protected static OWScene[] AllScenes = new OWScene[] { };

        protected abstract bool IsPersistent { get; }
        protected abstract OWScene[] Scenes { get; }

        private bool isPersistentBehaviourSetUp;

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
            if (isPersistentBehaviourSetUp || typeof(TBehaviour) == typeof(NomaiVRModule.EmptyBehaviour))
            {
                return;
            }

            Logs.WriteInfo($"Creating NomaiVR behaviour for {GetType().Name}");
            var gameObject = new GameObject();
            gameObject.AddComponent<TBehaviour>();

            if (IsPersistent)
            {
                gameObject.AddComponent<PersistObject>();
                isPersistentBehaviourSetUp = true;
            }
        }

        private void SetupPatch()
        {
            if (typeof(TPatch) == typeof(NomaiVRModule.EmptyPatch))
            {
                return;
            }

            Logs.WriteInfo($"Applying NomaiVR patches for {GetType().Name}");
            var patch = new TPatch();
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
