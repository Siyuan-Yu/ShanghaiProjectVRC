
using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Inventory
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class InventoryGrid : UdonSharpBehaviour
    {
        [SerializeField, Required] private SpriteRenderer spriteRenderer;

        private Item assignedItem;
        private string itemInteractStr;

        private InventoryManager inventoryManager;

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
            spriteRenderer.sprite = item.icon;
            assignedItem = item; 
            itemInteractStr = assignedItem.useHintText;
        }

        public void GridInteract()
        {
            switch (assignedItem.itemCategory)
            {
                case ItemCategory.Food:
                    break;
                case ItemCategory.Item:
                    break;
                case ItemCategory.NonUsable:
                    //TODO: To have a Tip Manager?
                    break;
            }
        }
    }
}

