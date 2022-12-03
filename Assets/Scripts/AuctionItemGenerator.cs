
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
/*
        public GameObject[] allUnits;
        */
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
/*            curVirtualTimeMinute = (int)clockUdon.GetProgramVariable("localTimeMinute");*/

            if (canDoAuction)
            {

                auctionInfoUI.text = "START";

                debugObject.SetActive(false);

                selectedItem = VRCInstantiate(prepItems[itemSelectionIndex]);
                    
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
/*
                if (curVirtualTimeMinute > 0 && curVirtualTimeMinute < 12)
                {
                    // selectedItem.transform
                    Debug.Log("Make it Rotate Here");
                }*/

/*                else if(curVirtualTimeMinute >= 12 && curVirtualTimeMinute < 18)
                {
                    selectedItem.transform.position =
                        Vector3.Lerp(selectedItem.transform.position, allUnits[0].transform.position, Time.deltaTime * 3);
                }

                else if(curVirtualTimeMinute > 20 && curVirtualTimeMinute < 21)
                {
                    //结束一次拍卖了，大家都归位
                    canRandom = true;
                    canDoAuction = false;
                    dayNightControllerUdon.SetProgramVariable("canAuction", true);
                }*/

        }

    }
}