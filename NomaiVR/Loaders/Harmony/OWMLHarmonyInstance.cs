using OWML.Common;
using System;
using System.Reflection;

namespace NomaiVR.Loaders
{
    public class OWMLHarmonyInstance : IHarmonyInstance
    {
        private IModHelper modHelper;
        public OWMLHarmonyInstance(IModHelper modHelper)
        {
            this.modHelper = modHelper;
        }

        public void AddPostfix(MethodBase original, Type patchType, string patchMethodName) =>
            modHelper.HarmonyHelper.AddPostfix(original, patchType, patchMethodName);

        public void AddPrefix(MethodBase original, Type patchType, string patchMethodName) =>
            modHelper.HarmonyHelper.AddPrefix(original, patchType, patchMethodName);
    }
}
