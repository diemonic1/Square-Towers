using UnityEngine;

namespace Utils
{
    public static class Utils
    {
        public static Color HexToColor(string hex)
        {
            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
                return color;

            Debug.LogError($"Invalid HEX color: {hex}");
            return Color.white;
        }
        
        public static string ToHex(this Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }
    }
}