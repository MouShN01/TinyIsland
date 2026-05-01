using System;
using UnityEngine;

namespace TinyIsland.Player
{
    public sealed class PlayerWoodInventory : MonoBehaviour
    {
        [SerializeField] private int woodCount;

        public event Action<int> WoodCountChanged;

        public int WoodCount => woodCount;

        public void AddWood(int amount)
        {
            if (amount <= 0)
                return;

            woodCount += amount;
            WoodCountChanged?.Invoke(woodCount);
        }

        public bool TrySpendWood(int amount)
        {
            if (amount <= 0)
                return true;

            if (woodCount < amount)
                return false;

            woodCount -= amount;
            WoodCountChanged?.Invoke(woodCount);

            return true;
        }
    }
}
