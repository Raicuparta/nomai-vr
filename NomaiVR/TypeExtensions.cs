using System;
using System.Linq;
using System.Reflection;

namespace NomaiVR
{
	public static class TypeExtensions
	{
		private const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

		public static MethodInfo GetAnyMethod(this Type type, string name) =>
			type.GetMethod(name, flags) ??
			type.BaseType?.GetMethod(name, flags) ??
			type.BaseType?.BaseType?.GetMethod(name, flags);

		public static MemberInfo GetAnyMember(this Type type, string name) =>
			type.GetMember(name, flags).FirstOrDefault() ??
			type.BaseType?.GetMember(name, flags).FirstOrDefault() ??
			type.BaseType?.BaseType?.GetMember(name, flags).FirstOrDefault();

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
