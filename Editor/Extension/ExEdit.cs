using System;
using UnityEngine;

namespace JJFramework.Editor.Extension
{
    public static class ExEdit
    {
        public static T ToEnum<T>(this string value)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, ignoreCase: false);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
