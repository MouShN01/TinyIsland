using TinyIsland.Player;
using UnityEngine;

namespace TinyIsland.Interaction
{
    public readonly struct PickupContext
    {
        public PickupContext(GameObject collector, PlayerWoodInventory woodInventory)
        {
            Collector = collector;
            CollectorTransform = collector != null ? collector.transform : null;
            WoodInventory = woodInventory;
        }

        public GameObject Collector { get; }
        public Transform CollectorTransform { get; }
        public PlayerWoodInventory WoodInventory { get; }
    }

    public interface IPickupable
    {
        Transform PickupTransform { get; }
        bool CanPickup(PickupContext context);
        void Pickup(PickupContext context);
    }
}
