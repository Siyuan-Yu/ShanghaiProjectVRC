using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace TryScripts
{
    public class DayNightEventSystem : UdonSharpBehaviour
    {
        public int virtualTimeHour;

        [Header("和昼夜相关")]

        public bool isDay;

        public int dayHourTime;
        public int nightHourTime;

        public Material daySkybox;
        public Material nightSkybox;
        public UdonBehaviour clockUdon;

        [Header("和拍卖相关")]

        public Text auctionInfoUI;

        public int firstAuctionTime;
        public int secondAuctionTime;

        public UdonBehaviour auctionUdon;
        public bool canAuction;

        //public UdonSharpBehaviour[] doors;
        public GameObject door1;
        Collider _door1Collider;

        public GameObject _btn1;
        public GameObject _btn2;
        public GameObject _btn3;
        public GameObject _btn4;



        // public Clock clockUdon;
        void Start()
        {
            isDay = false;
            canAuction = true;

            door1.SetActive(false);
            // virtualTimeHour = clockUdon.localTimeHour;


            //❗️不知道怎么udon findgameobjectswithtag
            // doors = GameObject.FindGameObjectsWithTag("rm_door");

            //
            _btn1.SetActive(false);
            // _btn2.SetActive(false);
            // _btn3.SetActive(false);
            // _btn4.SetActive(false);


        }

        void Update()
        {

            virtualTimeHour = (int)clockUdon.GetProgramVariable("localTimeHour");

            // in the Day
            if (virtualTimeHour > dayHourTime && virtualTimeHour < nightHourTime)
            {
                isDay = true;
                RenderSettings.skybox = daySkybox;


                //为了方便看，把这个门变成紫色了
                door1.SetActive(true);
            }

            // in the Night
            else
            {
                isDay = false;
                RenderSettings.skybox = nightSkybox;
                door1.SetActive(false);
                //门collider消失
                // _door1Collider.enabled = false;
                //
                // ButtonDeActivated();
            }

            CheckAuction();

        }



        void CheckAuction()
        {


            //in the Day

            if (isDay)
            {
                if(virtualTimeHour > firstAuctionTime - 2 && virtualTimeHour <= firstAuctionTime - 1)
                {
                    auctionInfoUI.text = "First Auction Will Start In One Hour";
                }

                else if (virtualTimeHour > firstAuctionTime - 1 && virtualTimeHour < firstAuctionTime + 1)
                {
                    if (canAuction)
                    {
                        auctionInfoUI.text = "Start First Auction";
                        canAuction = false;
                        auctionUdon.SetProgramVariable("canDoAuction", true);
                        
                        //按理来说Button的Active应该加在这里
                        ButtonActivated();
                        
                    }
                }

                // 给了2个小时的时间准备下一个auction，因此每个auction之间至少间隔2小时
                else if (virtualTimeHour >= firstAuctionTime + 1 && virtualTimeHour < firstAuctionTime + 2)
                {
                    //reset这个bool

                    auctionInfoUI.text = "First Auction is Over";

                    canAuction = true;
                    
                    //然后Button的DeActive应该加在这里
                    ButtonDeActivated();
                }


                else if(virtualTimeHour > secondAuctionTime - 2 && virtualTimeHour <= secondAuctionTime - 1)
                {
                    auctionInfoUI.text = "Second Auction Will Start In One Hour";
                }


                else if (virtualTimeHour > secondAuctionTime - 1 && virtualTimeHour < secondAuctionTime + 1)
                {
                    if (canAuction)
                    {
                        canAuction = false;

                        auctionInfoUI.text = "Start Second Auction";

                        auctionUdon.SetProgramVariable("canDoAuction", true);
                        
                        //同样Button的Active应该加在这里
                        ButtonActivated();
                        
                    }
                }

                else if (virtualTimeHour >= secondAuctionTime + 1)
                {
                    auctionInfoUI.text = "Second Auction is Over";

                    canAuction = true;
                    
                    //然后Button的DeActive应该加在这里
                    ButtonDeActivated();
                }

                else
                {
                    auctionInfoUI.text = "No Recent Auction";
                }

            }


            // In The Night
            else
            {
                auctionInfoUI.text = virtualTimeHour.ToString() + ",  Now Night, No Auction";
            }
         
        }

        void ButtonActivated()
        {
            _btn1.SetActive(true);
            // _btn2.SetActive(true);
            // _btn3.SetActive(true);
            // _btn4.SetActive(true);
        }

        void ButtonDeActivated()
        {
            _btn1.SetActive(false);
            // _btn2.SetActive(false);
            // _btn3.SetActive(false);
            // _btn4.SetActive(false);
        }


    }
}