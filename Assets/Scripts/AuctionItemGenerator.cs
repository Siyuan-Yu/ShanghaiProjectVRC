
using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
// using Random = UnityEngine.Random;


namespace TryScripts
{
    public class AuctionItemGenerator : UdonSharpBehaviour
    {

        public Text auctionInfoUI;

        public GameObject[] prepItems;
        public GameObject selectedItem;
        public int itemSelectionIndex;

        public GameObject showItemPosition;

        public GameObject AllUnits;
        public GameObject[] allUnits;

        public bool canDoAuction;

        private float auctionDisplayStartTime;
        private bool isDisplayingItem;
        private const float AuctionDisplayDuration = 15f;


        void Start()
        {
            Transform[] childTransforms = AllUnits.GetComponentsInChildren<Transform>();
            allUnits = new GameObject[childTransforms.Length - 1];  
            int index = 0;
            foreach (Transform child in childTransforms)
            {
                if (child != AllUnits.transform)
                {
                    allUnits[index] = child.gameObject;
                    index++;
                }
            }
            
            canDoAuction = false;
            
            itemSelectionIndex = 0;
            foreach (GameObject item in prepItems)
            {
                item.SetActive(false);
            }

            isDisplayingItem = false;
        }
        
        private void Update()
        {
            if (canDoAuction && !isDisplayingItem)
            {
                DisplayAuctionItem();
                canDoAuction = false;
            }

            // Check if the currently displaying item's time is up
            if (isDisplayingItem && (Time.time - auctionDisplayStartTime) >= AuctionDisplayDuration)
            {
                StopItemRotation();
            }
        }

        private void DisplayAuctionItem()
        {
            if (itemSelectionIndex < prepItems.Length)
            {
                ResetAllUnitClickCounts();
                selectedItem = prepItems[itemSelectionIndex];
                selectedItem.SetActive(true);
                selectedItem.GetComponent<AuctionItem>().isBought = false;
                selectedItem.transform.position = showItemPosition.transform.position;

                auctionInfoUI.text = "拍卖开始: " + selectedItem.name;

                itemSelectionIndex++;
                isDisplayingItem = true;
                auctionDisplayStartTime = Time.time;
            }
            else
            {
                itemSelectionIndex = 0;
            }
        }
        
        private void ResetAllUnitClickCounts()
        {
            // Traverse allUnits and reset the clickNum for each UnitClickCounter component
            foreach (GameObject unit in allUnits)
            {
                UnitClickCounter clickCounter = unit.GetComponent<UnitClickCounter>();
                if (clickCounter != null)
                {
                    Debug.Log("FBGSUAO");
                    clickCounter.clickNum = 0;
                }
            }
        }

        private void StopItemRotation()
        {
            if (selectedItem != null)
            {
                var auctionItem = selectedItem.GetComponent<AuctionItem>();
                auctionItem.isBought = true;
                isDisplayingItem = false;
            }
        }
    }
}