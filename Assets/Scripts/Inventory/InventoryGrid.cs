
using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Inventory
{
    public class InventoryGrid : UdonSharpBehaviour
    {
        [SerializeField, Required] private Image iconImage;
       // [SerializeField, Required] private SpriteRenderer spriteRenderer;

        private Item assignedItem;
        private string itemInteractStr;

        private InventoryManager inventoryManager;

        [SerializeField, Required] private Button button;
        
        private bool isSelected;

        private void Start()
        {
            inventoryManager = transform.parent.GetComponent<InventoryManager>();

            if (!inventoryManager)
            {
                Debug.LogError($"Inventory Manager not found on {name}");
            }
        }
        
        public void AssignItem(Item item)
        {
            iconImage = item.icon;
            assignedItem = item; 
            itemInteractStr = assignedItem.useHintText;
        }

        public void GridInteract()
        {
            
        }
        
    }
}

