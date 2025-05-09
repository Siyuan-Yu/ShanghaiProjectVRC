
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Inventory
{
    public class InventoryBag : UdonSharpBehaviour
    {
        public InventoryManager inventory;

        public LayerMask objectLayers = 13;
        private void OnTriggerEnter(Collider other)
        {
            if(!inventory) return;
            
            if(!inventory.HasSpace()) return;
            
            if (((1 << other.gameObject.layer) & objectLayers.value) != 0)
            {
                inventory.AddToInventory(other.gameObject.GetComponent<Item>());
            }
        }
    }
}

