using UnityEngine;
using TinyIsland.Interaction;

namespace TinyIsland.Wood
{
    public sealed class WoodPickup : MonoBehaviour, IPickupable
    {
        [SerializeField] private int woodAmount = 1;

        public Transform PickupTransform => transform;

        public bool CanPickup(PickupContext context)
        {
            return context.WoodInventory != null && woodAmount > 0;
        }

        public void Pickup(PickupContext context)
        {
            context.WoodInventory.AddWood(woodAmount);
            Destroy(gameObject);
        }
    }
}
