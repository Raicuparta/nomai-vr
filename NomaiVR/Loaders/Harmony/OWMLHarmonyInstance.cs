using System;
using System.Reflection;
using OWML.Common;

namespace NomaiVR.Loaders.Harmony
{
    public class OwmlHarmonyInstance : IHarmonyInstance
    {
        private readonly IModHelper modHelper;
        public OwmlHarmonyInstance(IModHelper modHelper)
        {
            this.modHelper = modHelper;
        }

        public void AddPostfix(MethodBase original, Type patchType, string patchMethodName) =>
            modHelper.HarmonyHelper.AddPostfix(original, patchType, patchMethodName);

        public void AddPrefix(MethodBase original, Type patchType, string patchMethodName) =>
            modHelper.HarmonyHelper.AddPrefix(original, patchType, patchMethodName);
    }
}
