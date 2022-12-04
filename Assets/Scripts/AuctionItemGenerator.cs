
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
        
        public GameObject[] allUnits;
        // UdonBehaviour auctionItem
        // public AuctionItem 
        
        public bool canDoAuction;

        public GameObject debugObject;

/*        public bool canRandom;

        public UdonBehaviour clockUdon;
        public int curVirtualTimeHour;
        public int curVirtualTimeMinute;
        */
        
/*        public UdonBehaviour dayNightControllerUdon;*/


        void Start()
        {
            itemSelectionIndex = 0;
            canDoAuction = false;
            /*canRandom = true;*/
        }

        private void Update()
        {
            if (canDoAuction)
            {
                auctionInfoUI.text = "START";

                debugObject.SetActive(false);

                selectedItem = VRCInstantiate(prepItems[itemSelectionIndex]);
                // selectedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
                selectedItem.transform.position = showItemPosition.transform.position;
                selectedItem.GetComponent<AuctionItem>().goToUnitObject = allUnits[0];
                
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