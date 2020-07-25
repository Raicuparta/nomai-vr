using System.Reflection;

namespace NomaiVR
{
    public abstract class NomaiVRPatch
    {
        protected void Pre<T>(string methodName, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<T>(methodName, GetType(), patchMethodName);
        }

        protected void Pre(MethodBase method, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPrefix(method, GetType(), patchMethodName);
        }

        protected void Post<T>(string methodName, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPostfix<T>(methodName, GetType(), patchMethodName);
        }

        protected void Post(MethodBase method, string patchMethodName)
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
