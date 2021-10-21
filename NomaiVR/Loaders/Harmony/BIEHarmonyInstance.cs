#if !OWML
extern alias BepInEx;
using BepInEx::HarmonyLib;
using System;
using System.Reflection;

namespace NomaiVR.Loaders
{
    public class BIEHarmonyInstance : IHarmonyInstance
    {
        private BepInEx::HarmonyLib.Harmony harmonyInstance;

        public BIEHarmonyInstance(BepInEx::HarmonyLib.Harmony harmonyInstance)
        {
            this.harmonyInstance = harmonyInstance;
        }

        public void AddPrefix(MethodBase original, Type patchType, string patchMethodName)
        {
            var prefix = TypeExtensions.GetAnyMethod(patchType, patchMethodName);
            if (prefix == null)
            {
                Logs.WriteError($"Error in {nameof(AddPrefix)}: {patchType.Name}.{patchMethodName} is null.");
                return;
            }
            Patch(original, prefix, null, null);
        }

        public void AddPostfix(MethodBase original, Type patchType, string patchMethodName)
        {
            var postfix = TypeExtensions.GetAnyMethod(patchType, patchMethodName);
            if (postfix == null)
            {
                Logs.WriteError($"Error in {nameof(AddPostfix)}: {patchType.Name}.{patchMethodName} is null.");
                return;
            }
            Patch(original, null, postfix, null);
        }

        private void Patch(MethodBase original, MethodInfo prefix, MethodInfo postfix, MethodInfo transpiler)
        {
            if (original == null)
            {
                Logs.WriteError($"Error in {nameof(Patch)}: original MethodInfo is null.");
                return;
            }
            var prefixMethod = prefix == null ? null : new HarmonyMethod(prefix);
            var postfixMethod = postfix == null ? null : new HarmonyMethod(postfix);
            var transpilerMethod = transpiler == null ? null : new HarmonyMethod(transpiler);
            var fullName = $"{original.DeclaringType}.{original.Name}";
            try
            {
                harmonyInstance.Patch(original, prefixMethod, postfixMethod, transpilerMethod);
                Logs.Write($"Patched {fullName}!");
            }
            catch (Exception ex)
            {
                Logs.WriteError($"Exception while patching {fullName}: {ex}");
            }
        }
    }
}
#endif