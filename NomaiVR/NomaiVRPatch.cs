using System.Reflection;

namespace NomaiVR
{
    public abstract class NomaiVRPatch
    {
        protected void Prefix<T>(string methodName, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<T>(methodName, GetType(), patchMethodName);
        }

        protected void Prefix(MethodBase method, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPrefix(method, GetType(), patchMethodName);
        }

        protected void Postfix<T>(string methodName, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPostfix<T>(methodName, GetType(), patchMethodName);
        }

        protected void Postfix(MethodBase method, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPostfix(method, GetType(), patchMethodName);
        }

        public static void Empty<T>(string methodName)
        {
            NomaiVR.Helper.HarmonyHelper.EmptyMethod<T>(methodName);
        }

        public static void Empty(MethodBase method)
        {
            NomaiVR.Helper.HarmonyHelper.EmptyMethod(method);
        }

        public abstract void ApplyPatches();
    }
}
