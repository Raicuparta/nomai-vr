using System;
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

        public void AddPrefix<T>(string methodName, Type patchType, string patchMethodName) =>
            AddPrefix(GetMethod<T>(methodName), patchType, patchMethodName);

        public void AddPrefix(MethodBase original, Type patchType, string patchMethodName) => 
            NomaiVR.Helper.HarmonyHelper.AddPrefix(original, patchType, patchMethodName);

        public void AddPostfix<T>(string methodName, Type patchType, string patchMethodName) =>
            AddPostfix(GetMethod<T>(methodName), patchType, patchMethodName);

        public void AddPostfix(MethodBase original, Type patchType, string patchMethodName) =>
            NomaiVR.Helper.HarmonyHelper.AddPostfix(original, patchType, patchMethodName);

        public void EmptyMethod<T>(string methodName) =>
            EmptyMethod(GetMethod<T>(methodName));

        public void EmptyMethod(MethodBase methodInfo) =>
            AddPrefix(methodInfo, typeof(NomaiVRPatch), nameof(EmptyMethodPatch));

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

        public static bool EmptyMethodPatch() => false;

        public abstract void ApplyPatches();
    }
}
