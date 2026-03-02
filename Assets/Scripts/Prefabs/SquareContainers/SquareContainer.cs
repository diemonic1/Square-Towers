using UnityEngine;

namespace Prefabs.SquareContainers
{
    public abstract class SquareContainer : MonoBehaviour
    {
        public abstract EAddingSquareResult GetCanAddSquareStatus(SquareForBuilding square);

        public abstract void AddSquare(SquareForBuilding square, bool withAnimation = true);
    }
}