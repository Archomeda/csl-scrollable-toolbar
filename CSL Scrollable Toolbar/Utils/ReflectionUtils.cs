using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScrollableToolbar.Utils
{
    internal static class ReflectionUtils
    {
        internal static T GetPrivateField<T>(object obj, string name)
        {
            return (T)obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
        }

        internal static void SetPrivateField<T>(object obj, string name, T value)
        {
            obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        }

        internal static void InvokePrivateMethod(object obj, string name, params object[] args)
        {
            MethodInfo method = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(obj, args);
        }

        internal static T InvokePrivateMethod<T>(object obj, string name, params object[] args)
        {
            MethodInfo method = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)method.Invoke(obj, args);
        }

        internal static void InvokePrivateStaticMethod(Type type, string name, params object[] args)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, args);
        }

        internal static T InvokePrivateStaticMethod<T>(Type type, string name, params object[] args)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            return (T)method.Invoke(null, args);
        }
    }
}
