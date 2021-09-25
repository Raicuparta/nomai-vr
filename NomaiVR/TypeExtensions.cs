using System;
using System.Linq;
using System.Reflection;

namespace NomaiVR
{
	public static class TypeExtensions
	{
		private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

		public static MethodInfo GetAnyMethod(this Type type, string name) =>
			type.GetMethod(name, Flags) ??
			type.BaseType?.GetMethod(name, Flags) ??
			type.BaseType?.BaseType?.GetMethod(name, Flags);

		public static MemberInfo GetAnyMember(this Type type, string name) =>
			type.GetMember(name, Flags).FirstOrDefault() ??
			type.BaseType?.GetMember(name, Flags).FirstOrDefault() ??
			type.BaseType?.BaseType?.GetMember(name, Flags).FirstOrDefault();

		public static void SetValue(this object obj, string name, object value)
		{
			switch (obj.GetType().GetAnyMember(name))
			{
				case FieldInfo field:
					field.SetValue(obj, value);
					break;
				case PropertyInfo property:
					property.SetValue(obj, value, null);
					break;
			}
		}

		public static void Invoke(this object obj, string name, params object[] parameters) =>
			Invoke<object>(obj, name, parameters);

		public static T Invoke<T>(this object obj, string name, params object[] parameters) =>
			(T)obj.GetType().GetAnyMethod(name)?.Invoke(obj, parameters);

		public static T Invoke<T>(this Type type, string name, params object[] parameters) =>
			(T)type.GetAnyMethod(name).Invoke(null, parameters);
	}
}
