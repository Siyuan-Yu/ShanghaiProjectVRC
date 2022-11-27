
using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
// using Random = UnityEngine.Random;


namespace TryScripts
{
    public class AuctionItemGenerator : UdonSharpBehaviour
    {
        
        public GameObject[] prepItems;
        public GameObject selectedItem;
        
        public bool canDoAuction;
        public bool canRandom;


        void Start()
        {
            canDoAuction = false;
            canRandom = true;
        }

        private void Update()
        {
            if (canDoAuction)
            {
                if (canRandom)
                {
                    // VRC.SDKBase.Utilities.ShuffleArray(prepItems);
                    // selectedItem =  Instantiate(prepItems[0]);
                    // RemoveAt(ref prepItems, 0);

                    // prepItems
                    canRandom = false;
                }
            }
        }

        // static void RemoveAt<T>(ref T[] arr, int index)
        // {
        //     for (int a = index; a < arr.Length - 1; a++)
        //     {
        //         // moving elements downwards, to fill the gap at [index]
        //         arr[a] = arr[a + 1];
        //     }
        //     // finally, let's decrement Array's size by one
        //     Array.Resize(ref arr, arr.Length - 1);
        // }
        //
        
    }
}