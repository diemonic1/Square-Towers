using System;
using System.Collections.Generic;
using Prefabs.SquareContainers;
using UnityEngine;

namespace Services.SavingService.SavingModels
{
    [Serializable]
    public class TowerForSave
    {
        public List<SquareForSave> Squares = new();
        public Vector3 Position;
        
        public TowerForSave(List<SquareForSave> squares, Vector3 position)
        {
            Squares = squares;
            Position = position;
        }
    }
}