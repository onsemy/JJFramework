using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JJFramework.Runtime.Extension
{
    [DisallowMultipleComponent]
    public static class ExSystem
    {
        public static T ToEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, false);
            }
            catch
            {
                return default(T);
            }
        }

        public static string Format(this string s, params object[] args)
        {
            return string.Format(s, args);
        }

        public static string ToUnixPath(this string fpath)
        {
            // "a\\b.txt" => "a/b.txt"
            return fpath.Replace('\\', '/');
        }

        public static Type GetMemberType(this MemberInfo info)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Event:
                    var e = info as EventInfo;
                    return e?.EventHandlerType;

                case MemberTypes.Field:
                    var f = info as FieldInfo;
                    return f?.FieldType;

                case MemberTypes.Method:
                    var m = info as MethodInfo;
                    return m?.ReturnType;

                case MemberTypes.Property:
                    var p = info as PropertyInfo;
                    return p?.PropertyType;

                case MemberTypes.Constructor:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    return null;
            }
        }

        public static void SetValue(this MemberInfo info, object obj, object value)
        {
            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    var f = (info as FieldInfo);
                    f.SetValue(obj, value);
                    break;

                case MemberTypes.Property:
                    var p = (info as PropertyInfo);
                    p.SetValue(obj, value, null);
                    break;

                case MemberTypes.Constructor:
                case MemberTypes.Method:
                case MemberTypes.Event:
                case MemberTypes.TypeInfo:
                case MemberTypes.Custom:
                case MemberTypes.NestedType:
                case MemberTypes.All:
                default:
                    break;
            }
        }

        public static IList<T> Shuffle<T>(this IList<T> list, bool isSeedRand = true)
        {
            if (isSeedRand)
            {
                UnityEngine.Random.InitState(DateTime.Now.Millisecond);
            }

            int count = list.Count;
            while (count > 1)
            {
                --count;

                var random = UnityEngine.Random.Range(0, count + 1);

                var value = list[random];
                list[random] = list[count];
                list[count] = value;
            }

            return list;
        }

        public static T Random<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                throw new IndexOutOfRangeException("리스트가 비어있습니다!");
            }

            if (list.Count == 1)
            {
                return list[0];
            }

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T RandomOrDefault<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return default(T);
            }

            return list.Random();
        }

        public static T Pop<T>(this IList<T> list, int index)
        {
            if (list.Count == 0)
            {
                Debug.LogError("리스트가 비어있습니다!");
                return default(T);
            }

            if (list.Count <= index ||
                index < 0)
            {
                Debug.LogError("범위를 벗어났습니다!");
                return default(T);
            }

            var value = list[index];

            list.RemoveAt(index);

            return value;
        }

        public static T PopLast<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                Debug.LogError("리스트가 비어있습니다!");
                return default(T);
            }

            var value = list[list.Count - 1];

            list.RemoveAt(list.Count - 1);

            return value;
        }
    }
}
