using UnityEngine;
using System.Linq;
using System;

namespace NomaiVR
{
    public abstract class NomaiVRModule<Behaviour, Patch>
        where Patch : NomaiVRPatch, new()
        where Behaviour : MonoBehaviour
    {
        public static Type EmptyPatch = typeof(EmptyPatchClass);
        public static Type EmptyBehaviour = typeof(EmptyBehaviourClass);

        private bool _isPersistent;
        private OWScene[] _scenes;

        public NomaiVRModule(bool isPersistent, OWScene[] scenes)
        {
            _isPersistent = isPersistent;
            _scenes = scenes;

            if (scenes.Contains(LoadManager.GetCurrentScene()))
            {
                SetupBehaviour();
            }

            LoadManager.OnCompleteSceneLoad += OnSceneLoad;

            SetupPatch();
        }

        private void OnSceneLoad(OWScene originalScene, OWScene loadScene)
        {
            if (_scenes.Contains(loadScene))
            {
                SetupBehaviour();
            }
        }

        private void SetupBehaviour()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Behaviour>();
            if (_isPersistent)
            {
                gameObject.AddComponent<PersistObject>();
            }
        }

        private void SetupPatch()
        {
            var patch = new Patch();
            patch.ApplyPatches();
        }

        public class EmptyPatchClass : NomaiVRPatch
        {
            public override void ApplyPatches()
            { }
        }

        public class EmptyBehaviourClass : MonoBehaviour
        { }
    }
}
