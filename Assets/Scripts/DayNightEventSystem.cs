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

       public UdonSharpBehaviour[] doors;
       public GameObject door1;
       Collider _door1Collider;
       
        
        
        // public Clock clockUdon;
        void Start()
        {
            canAuction = true;
            // virtualTimeHour = clockUdon.localTimeHour;
           

           //❗️不知道怎么udon findgameobjectswithtag
            // doors = GameObject.FindGameObjectsWithTag("rm_door"); 

             _door1Collider = door1.GetComponent<Collider>();
        }

        void Update()
        {

            virtualTimeHour = (int)clockUdon.GetProgramVariable("localTimeHour");

            // in the Day
            if (virtualTimeHour >= dayHourTime && virtualTimeHour < nightHourTime)
            {
                RenderSettings.skybox = daySkybox;
                _door1Collider.enabled = true;

                if (virtualTimeHour > firstAuctionTime)
                {
                    DoAuction();
                }

                //Activate Buttons
            }

            // in the Night
            else
            {
                RenderSettings.skybox = nightSkybox;

                //门collider消失
               
                _door1Collider.enabled = false;
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

    
    }
}