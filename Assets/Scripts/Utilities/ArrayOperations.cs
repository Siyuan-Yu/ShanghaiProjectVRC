
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

namespace Utilities
{
    public static class Array
    {
        public static T[] RemoveFrom<T>(T[] array, T targetObject)
        {
            // Create a new array with one less slot
            var newArray = new T[array.Length - 1];
            var newIndex = 0;

            // Copy all objects except the target object
            foreach (var item in array)
            {
                if (EqualityComparer<T>.Default.Equals(item, targetObject))
                    continue;

                newArray[newIndex] = item;
                newIndex++;
            }

            // Return the new array
            return newArray;
        }

        public static T[] AddTo<T>(T[] array, T newObject)
        {
            var newArray = new T[array.Length + 1];

            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }

            newArray[newArray.Length - 1] = newObject;

            return newArray;
        }

        public static bool IntContains(int[] array, int targetValue)
        {
            foreach (int item in array)
            {
                if (item == targetValue)
                {
                    return true; // Found the value
                }
            }

            return false; // Value not found
        }

        public static bool StringContains(string[] array, string targetValue)
        {
            foreach (string item in array)
            {
                if (item == targetValue)
                {
                    return true; // Found the value
                }
            }

            return false; // Value not found
        }

        public static bool GameObjectContains(GameObject[] array, GameObject targetValue)
        {
            foreach (GameObject item in array)
            {
                if (item == targetValue)
                {
                    return true; // Found the value
                }
            }

            return false; // Value not found
        }
    }

}
