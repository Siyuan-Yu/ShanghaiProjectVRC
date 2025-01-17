
using System.Collections.Generic;
using UdonSharp;

namespace Utilities
{
    public static class Array
    {
        public static T[] RemoveFrom<T>(T[] inventoryList, T targetObject)
        {
            // Create a new array with one less slot
            var newArray = new T[inventoryList.Length - 1];
            var newIndex = 0;

            // Copy all objects except the target object
            foreach (var item in inventoryList)
            {
                if (EqualityComparer<T>.Default.Equals(item, targetObject)) 
                    continue;

                newArray[newIndex] = item;
                newIndex++;
            }

            // Return the new array
            return newArray;
        }
        
        public static T[] AddTo<T>(T[] inventoryList, T newObject)
        {
            // Create a new array with one additional slot
            var newArray = new T[inventoryList.Length + 1];

            // Copy the existing array to the new array
            for (int i = 0; i < inventoryList.Length; i++)
            {
                newArray[i] = inventoryList[i];
            }

            // Add the new object to the last slot
            newArray[newArray.Length - 1] = newObject;

            // Return the new array
            return newArray;
        }
    }
    
    
}
