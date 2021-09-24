using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NomaiVR
{
    public abstract class NomaiVRPatch
    {
        protected void Prefix<T>(string methodName, string patchMethodName)
        {
            AddPrefix<T>(methodName, GetType(), patchMethodName);
        }

        protected void Prefix(MethodBase method, string patchMethodName)
        {
            AddPrefix(method, GetType(), patchMethodName);
        }

        protected void Postfix<T>(string methodName, string patchMethodName)
        {
            AddPostfix<T>(methodName, GetType(), patchMethodName);
        }

        protected void Postfix(MethodBase method, string patchMethodName)
        {
            AddPostfix(method, GetType(), patchMethodName);
        }

        public void Empty<T>(string methodName)
        {
            EmptyMethod<T>(methodName);
        }

        public static void Empty(MethodBase method)
        {
            // NomaiVR.Helper.HarmonyHelper.EmptyMethod(method);
        }

        public void AddPrefix<T>(string methodName, Type patchType, string patchMethodName) =>
    AddPrefix(GetMethod<T>(methodName), patchType, patchMethodName);

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

        public void AddPostfix<T>(string methodName, Type patchType, string patchMethodName) =>
            AddPostfix(GetMethod<T>(methodName), patchType, patchMethodName);

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

        public void EmptyMethod<T>(string methodName) =>
            EmptyMethod(GetMethod<T>(methodName));

        public void EmptyMethod(MethodBase methodInfo) =>
            Transpile(methodInfo, typeof(NomaiVRPatch), nameof(EmptyMethodPatch));

        public static IEnumerable<CodeInstruction> EmptyMethodPatch(IEnumerable<CodeInstruction> _) =>
            new List<CodeInstruction>();

        public void Transpile(MethodBase original, Type patchType, string patchMethodName)
        {
            var patchMethod = TypeExtensions.GetAnyMethod(patchType, patchMethodName);
            if (patchMethod == null)
            {
                Logs.WriteError($"Error in {nameof(Transpile)}: {patchType.Name}.{patchMethodName} is null.");
                return;
            }
            Patch(original, null, null, patchMethod);
        }

        private MethodInfo GetMethod<T>(string methodName)
        {
            var fullName = $"{typeof(T).Name}.{methodName}";
            try
            {
                Logs.Write($"Getting method {fullName}");
                var result = TypeExtensions.GetAnyMethod(typeof(T), methodName);
                if (result == null)
                {
                    Logs.WriteError($"Error - method {fullName} not found.");
                }
                return result;
            }
            catch (Exception ex)
            {
                Logs.WriteError($"Exception while getting method {fullName}: {ex}");
                return null;
            }
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
                NomaiVR.HarmonyInstance.Patch(original, prefixMethod, postfixMethod, transpilerMethod);
                Logs.Write($"Patched {fullName}!");
            }
            catch (Exception ex)
            {
                Logs.WriteError($"Exception while patching {fullName}: {ex}");
            }
        }
        public abstract void ApplyPatches();
    }
}
