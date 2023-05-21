
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
        public int prepItemLength;
        public GameObject selectedItem;
        public int itemSelectionIndex;

        public GameObject showItemPosition;
        
        public GameObject[] allUnits;
        // UdonBehaviour auctionItem
        // public AuctionItem 
        
        public bool canDoAuction;

/*        public bool canRandom;

        public UdonBehaviour clockUdon;
        public int curVirtualTimeHour;
        public int curVirtualTimeMinute;
        */
        
/*        public UdonBehaviour dayNightControllerUdon;*/


        void Start()
        {
            prepItemLength = prepItems.Length;
            itemSelectionIndex = 0;
            for (int i = 0; i < prepItemLength; i++)
            {
                prepItems[i].SetActive(false);
            }
            canDoAuction = false;
            /*canRandom = true;*/
        }

        private void Update()
        {
            if (canDoAuction)
            {
                auctionInfoUI.text = "START";

                if (itemSelectionIndex < prepItemLength)
                {
                    // selectedItem = VRCInstantiate(prepItems[itemSelectionIndex]);
                    selectedItem = prepItems[itemSelectionIndex];
                    selectedItem.SetActive(true);
                    // selectedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
                    selectedItem.transform.position = showItemPosition.transform.position;
                    selectedItem.GetComponent<AuctionItem>().goToUnitIndex = 0;

                    int nowMinute, goalMinute;
                    nowMinute = VRC.SDKBase.Networking.GetNetworkDateTime().Minute;
                    if (nowMinute < 59)
                    {
                        goalMinute = nowMinute + 1;
                    }
                    else
                    {
                        goalMinute = 0;
                    }
                    
                    selectedItem.GetComponent<AuctionItem>().startRealTimeMinute = nowMinute;
                    selectedItem.GetComponent<AuctionItem>().goalRealTimeMinute = goalMinute ;
                }
                // VRCInstantiate(prepItems[itemSelectionIndex]);

                auctionInfoUI.text = "START" + prepItems[itemSelectionIndex].name + "" + prepItems[itemSelectionIndex].transform.position;
                
                if (itemSelectionIndex + 1 < prepItems.Length)
                {
                    itemSelectionIndex += 1;
                }
                else
                {
                    itemSelectionIndex = 0;
                }

                canDoAuction = false;
             }
        }
    }
}