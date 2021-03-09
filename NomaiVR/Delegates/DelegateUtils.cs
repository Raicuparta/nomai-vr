using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace NomaiVR.Delegates
{
    /// <summary>
    /// Offers a way to use Ref-Getters to avoid using reflection to access private fields in Update loops
    /// Code based on: https://stackoverflow.com/questions/16073091/is-there-a-way-to-create-a-delegate-to-get-and-set-values-for-a-fieldinfo
    /// </summary>
    public static class DelegateUtils
    {
        public delegate ref U RefGetter<T, U>(T obj);

        // The first call to this DynamicMethod will be really expensive due to compilation overhead, 
        // the subsequent calls should be as close as possible to a normale managed C# method call
        public static RefGetter<T, U> CreateRefGetter<T, U>(String s_field)
        {
            const BindingFlags bf = BindingFlags.NonPublic |
                                    BindingFlags.Instance |
                                    BindingFlags.Public |
                                    BindingFlags.DeclaredOnly;

            var fi = typeof(T).GetField(s_field, bf);
            if (fi == null)
                throw new MissingFieldException(typeof(T).Name, s_field);

            var s_name = "__refget_" + typeof(T).Name + "_fi_" + fi.Name;

            // workaround for using ref-return with DynamicMethod:
            //   a.) initialize with dummy return value
            var dm = new DynamicMethod(s_name, typeof(U), new[] { typeof(T) }, typeof(T), true);

            //   b.) replace with desired 'ByRef' return value
            dm.GetType().GetField("returnType", bf).SetValue(dm, typeof(U).MakeByRefType());

            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldflda, fi);
            il.Emit(OpCodes.Ret);

            return (RefGetter<T, U>)dm.CreateDelegate(typeof(RefGetter<T, U>));
        }
    }
}
