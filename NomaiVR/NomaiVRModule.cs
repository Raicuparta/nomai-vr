using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    public abstract class NomaiVRModule<Behaviour, Patch>
        where Patch : NomaiVRPatch, new()
        where Behaviour : MonoBehaviour
    {
        public NomaiVRModule()
        {
            NomaiVR.persistentParent.AddComponent<Behaviour>();
            var patch = new Patch();
            patch.ApplyPatches();
        }
    }

    public abstract class NomaiVRModule<Behaviour>
        where Behaviour : MonoBehaviour
    {
        public NomaiVRModule()
        {
            NomaiVR.persistentParent.AddComponent<Behaviour>();
        }
    }

    public abstract class NomaiVRModule
    {
        public NomaiVRModule()
        { }
    }
}
