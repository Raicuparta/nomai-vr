using System;
using System.Reflection;

namespace NomaiVR
{
    public static class PatchHelper
    {
        public static void Pre<T>(string methodName, Type patchType, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<T>(methodName, patchType, patchMethodName);
        }

        public static void Pre(MethodBase method, Type patchType, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPrefix(method, patchType, patchMethodName);
        }

        public static void Post<T>(string methodName, Type patchType, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPostfix<T>(methodName, patchType, patchMethodName);
        }

        public static void Post(MethodBase method, Type patchType, string patchMethodName)
        {
            NomaiVR.Helper.HarmonyHelper.AddPostfix(method, patchType, patchMethodName);
        }

        public static void Empty<T>(string methodName)
        {
            NomaiVR.Helper.HarmonyHelper.EmptyMethod<T>(methodName);
        }

        public static void Empty(MethodBase method)
        {
            NomaiVR.Helper.HarmonyHelper.EmptyMethod(method);
        }
    }
}
