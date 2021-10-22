using System;
using System.Reflection;

namespace NomaiVR.Loaders.Harmony
{
    public interface IHarmonyInstance
    {
        void AddPrefix(MethodBase original, Type patchType, string patchMethodName);
        void AddPostfix(MethodBase original, Type patchType, string patchMethodName);
    }
}
