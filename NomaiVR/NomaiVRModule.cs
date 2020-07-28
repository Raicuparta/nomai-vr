using OWML.Common;
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
            if (typeof(Behaviour) == typeof(NomaiVRModule.EmptyBehaviour))
            {
                return;
            }

            NomaiVR.Log($"Creating NomaiVR behaviour for ${GetType().Name}", MessageType.Info);
            var gameObject = new GameObject();
            gameObject.AddComponent<Behaviour>();

            if (IsPersistent)
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

            NomaiVR.Log($"Applying NomaiVR patches for ${GetType().Name}", MessageType.Info);
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
