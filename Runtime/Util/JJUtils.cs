using System;
using System.Globalization;
using UnityEngine;

namespace JJFramework.Runtime.Util
{
    public class JJUtils
    {
        public static Color32 HexToColor(string hex)
        {
            hex = hex.Replace("0x", "");
            hex = hex.Replace("#", "");

            if (hex.Length < 6 ||
                hex.Length > 8)
            {
                Debug.LogError($"[JJUtils|HexToColor] Parameter is invalid! - {hex}");
                return new Color32();
            }
            
            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            byte a = 255;
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }
            
            return new Color32(r, g, b, a);
        }
    }
}