using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NomaiVR.Delegates.Input
{
    public class Rumble
    {
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        private readonly static Type _rumbleType = typeof(RumbleManager).GetNestedType("Rumble", Flags);
        private readonly static MethodInfo _rumbleUpdate = _rumbleType?.GetMethod("Update", Flags);
        private readonly static MethodInfo _rumbleGetPower = _rumbleType?.GetMethod("GetPower", Flags);
        private readonly static MethodInfo _rumbleIsAlive = _rumbleType?.GetMethod("IsAlive", Flags);

        public readonly Action<float> Update;
        public readonly Func<Vector2> GetPower;
        public readonly Func<bool> IsAlive;

        public Rumble(object boundObject)
        {
            Update = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), boundObject, _rumbleUpdate);
            GetPower = (Func<Vector2>)Delegate.CreateDelegate(typeof(Func<Vector2>), boundObject, _rumbleGetPower);
            IsAlive = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), boundObject, _rumbleIsAlive);
        }
    }
}
