using System;
using UdonSharp;
using UnityEngine;
//using VRC.SDK3.Data;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using VRC.SDKBase;
using Array = ProjectUtilities.Array;

// ReSharper disable UseIndexFromEndExpression

namespace Inventory
{
    public class InventoryManager : UdonSharpBehaviour
    {
        //[SerializeField,ReadOnly] private DataList<GameObject> inventoryList = new DataList<GameObject>();
        [SerializeField, ReadOnly] private Item[] inventoryList = new Item[0];
        
        [SerializeField,ReadOnly] private InventoryGrid[] gridList = new InventoryGrid[0];

        /*[Title("Layout Setting")] [SerializeField]
        private float spreadTotalAngle = 180f;// Spread objects across 180 degrees (semicircle)
        [SerializeField]
        private float layOutRadius = 300f;

        private float _stagingYOffset = 0.5f;*/

        private void Start()
        {
            gridList = GetComponentsInChildren<InventoryGrid>();

            Debug.Log("Grid Length: "+gridList.Length);
            //_stagingYOffset = transform.position.y;

            /*foreach (var transform1 in GetComponentsInChildren<Transform>())
            {
                if (transform1.gameObject == gameObject) continue;
                AddToInventory(transform1);
            }*/
            //Debug.Log(inventoryList.GetValue(0).name);
        }

        [Button]
        private void TestCollectInventory()
        {
            gridList = GetComponentsInChildren<InventoryGrid>();
        }

        private void Update()
        {
            LayoutInventories();
        }

        public void AddToInventory(Item newObject)//, VRCPlayerApi player)
        {
            //if(Networking.LocalPlayer != player) return;
            //TODO
            var newObjectTransform = newObject.transform;

            var l = inventoryList.Length;
            var newInventoryList = new Item[l + 1];
            for (var i = 0; i < l; i++)
            {
                newInventoryList[i] = inventoryList[i];
            }
            newInventoryList[l] = newObject;
            
            inventoryList = newInventoryList;
            
            Debug.Log("Added to inventory: " + newObject.name + "Lenght: " + inventoryList.Length);
            
            newObjectTransform.parent = transform;
            
            newObject.gameObject.SetActive(false);

            LayoutInventories();
        }

        public void RemoveFromInventory(Item targetObject)
        {
            // Create a new array with one less slot
            var newArray = new Item[inventoryList.Length - 1];
            var newIndex = 0;

            // Copy all objects except the target object
            foreach (var t in inventoryList)
            {
                if (t == targetObject) continue;
                newArray[newIndex] = t;
                newIndex++;
            }

            // Replace the old array with the new array
            inventoryList = newArray;
            LayoutInventories();
        }

        public void LayoutInventories()
        {
            var gridIndex = 0;
            foreach (var item in inventoryList)
            {
                if (!item)
                {
                    Debug.LogWarning($"Item {gridIndex} is null");
                    continue;
                }

                if (!gridList[gridIndex])
                {
                    Debug.LogWarning($"Grid {gridIndex} is null");
                    continue;
                }
                Debug.Log($"Assign Grid {gridIndex} to {item.name}");
                gridList[gridIndex].AssignItem(item);
                gridIndex++;
                
                
            }
        }

        public bool HasSpace()
        {
            return inventoryList.Length < gridList.Length;
        }
        
        /*public void LayoutInventories()
        {
            var startAngle = -spreadTotalAngle / 2f;
            var spacing = spreadTotalAngle / (inventoryList.Length - 1); // Adjust spacing for semicircle

            for (var i = 0; i < inventoryList.Length; i++)
            {
                var angle = startAngle + spacing * i; // Calculate angle for each object
                var radians = Mathf.Deg2Rad * angle;
                var x = layOutRadius * Mathf.Sin(radians);
                if (float.IsNaN(x)) x = 0f;
                var z = layOutRadius * Mathf.Cos(radians);
                if (float.IsNaN(z)) z = 0f;
                inventoryList[i].localPosition = new Vector3(x, _stagingYOffset, z);
                inventoryList[i].parent = transform;
            }
        }*/

    }

    public enum ItemCategory
    {
        Food,
        Item,
        NonUsable
    }
}