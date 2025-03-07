using System;
using UdonSharp;
using UnityEngine;
//using VRC.SDK3.Data;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

// ReSharper disable UseIndexFromEndExpression

namespace Inventory
{
    public class InventoryManager : UdonSharpBehaviour
    {
        //[SerializeField,ReadOnly] private DataList<GameObject> inventoryList = new DataList<GameObject>();
        [SerializeField, ReadOnly] private Transform[] inventoryList = new Transform[0];

        [Title("Layout Setting")] [SerializeField]
        private float spreadTotalAngle = 180f;// Spread objects across 180 degrees (semicircle)
        [SerializeField]
        private float layOutRadius = 300f;

        private float _stagingYOffset = 0.5f;

        private void Start()
        {
            _stagingYOffset = transform.position.y;

            foreach (var transform1 in GetComponentsInChildren<Transform>())
            {
                if (transform1.gameObject == gameObject) continue;
                AddToInventory(transform1);
            }
            //Debug.Log(inventoryList.GetValue(0).name);
        }

        private void Update()
        {
            LayoutInventories();
        }

        public void AddToInventory(Transform newObject)
        {
            // Create a new array with one additional slot
            Transform[] newArray = new Transform[inventoryList.Length + 1];

            // Copy existing objects into the new array
            for (var i = 0; i < inventoryList.Length; i++)
            {
                newArray[i] = inventoryList[i];
            }

            // Add the new object to the end of the array
            newArray[newArray.Length - 1] = newObject;

            // Replace the old array with the new array
            inventoryList = newArray;

            LayoutInventories();
        }

        public void RemoveFromInventory(Transform targetObject)
        {
            // Create a new array with one less slot
            var newArray = new Transform[inventoryList.Length - 1];
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

        /*public void LayoutInventories()
        {
            var spacing = 360f / inventoryList.Length;

            for (var i = 0; i < inventoryList.Length; i++)
            {
                var radians = Mathf.Deg2Rad * (spacing * i);
                var x = layOutRadius * Mathf.Sin(radians);
                var z = layOutRadius * Mathf.Cos(radians);
                inventoryList[i].localPosition = new Vector3(x, _stagingYOffset, z);
                inventoryList[i].parent = transform;
            }
        }*/
        public void LayoutInventories()
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
        }

    }

    public enum ItemCategory
    {
        Food,
        Item,
        NonUsable
    }
}