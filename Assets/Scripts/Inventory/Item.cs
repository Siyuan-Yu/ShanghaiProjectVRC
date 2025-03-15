
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRRefAssist;

namespace Inventory
{
    [RequireComponent(typeof(Collider)),RequireComponent(typeof(Rigidbody))]
    public class Item : UdonSharpBehaviour
    {
        public string useHintText = "Use";
        public ItemCategory itemCategory;
        
        [Required]
        public Image icon;

        [Title("Inventory","TODO, How to find the correct one?")] [SerializeField]//, Required]
        private InventoryManager inventoryManager;

        public override void Interact()
        {
            if (!inventoryManager)
            {
                Debug.LogWarning("No Inventory Manager found on " + name);
            }
            Debug.Log($"{name} is interacting with {Networking.LocalPlayer.displayName}");
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            inventoryManager.AddToInventory(this, Networking.LocalPlayer);
            //TODO: FInd the local player and add it to the player's inventory
        }
    }
}

