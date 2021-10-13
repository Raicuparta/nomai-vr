using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NomaiVR.Loaders
{
    public interface IHarmonyInstance
    {
        void AddPrefix(MethodBase original, Type patchType, string patchMethodName);
        void AddPostfix(MethodBase original, Type patchType, string patchMethodName);
    }
}
