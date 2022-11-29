using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


namespace TryScripts
{
    public class DayNightEventSystem : UdonSharpBehaviour
    {
        public int virtualTimeHour;

        [Header("和昼夜相关")]
        public int dayHourTime;
        public int nightHourTime;

        public Material daySkybox;
        public Material nightSkybox;
        public UdonBehaviour clockUdon;

        [Header("和拍卖相关")]
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
            canAuction = true;
            // virtualTimeHour = clockUdon.localTimeHour;


            //❗️不知道怎么udon findgameobjectswithtag
            // doors = GameObject.FindGameObjectsWithTag("rm_door");

            // _door1Collider = door1.GetComponent<Collider>();
            //
            // _btn1.SetActive(false);
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
                RenderSettings.skybox = daySkybox;
                _door1Collider.enabled = true;
                //
                // if (virtualTimeHour > firstAuctionTime && virtualTimeHour < secondAuctionTime)
                // {
                //     DoAuction();
                //
                //     ButtonActivated();
                //
                // }
                // else
                // {
                //     ButtonDeActivated();
                // }
                
            }

            // in the Night
            else
            {
                RenderSettings.skybox = nightSkybox;

                //门collider消失
                // _door1Collider.enabled = false;
                //
                // ButtonDeActivated();
            }

        }



        void DoAuction()
        {
            if (canAuction)
            {
                canAuction = false;
                auctionUdon.SetProgramVariable("canDoAuction", true);
            }
        }

        void ButtonActivated()
        {
            _btn1.SetActive(true);
            _btn2.SetActive(true);
            _btn3.SetActive(true);
            _btn4.SetActive(true);
        }

        void ButtonDeActivated()
        {
            _btn1.SetActive(false);
            _btn2.SetActive(false);
            _btn3.SetActive(false);
            _btn4.SetActive(false);
        }


    }
}