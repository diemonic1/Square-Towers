using System;
using UnityEngine;
using Utils;

namespace Services.SavingService.SavingModels
{
    [Serializable]
    public class SquareForSave
    {
        public Vector3 Position;

        public Color Color => Utils.Utils.HexToColor(ColorString);
        
        public string ColorString;

        public SquareForSave(Vector3 position, Color color)
        {
            Position = position;
            ColorString = color.ToHex();
        }
    }
}