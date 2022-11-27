
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
        
        
        // public Clock clockUdon;
        void Start()
        {
            canAuction = true;
            // virtualTimeHour = clockUdon.localTimeHour;
        }

        void Update()
        {

            virtualTimeHour = (int)clockUdon.GetProgramVariable("localTimeHour");

            // in the Day
            if (virtualTimeHour > dayHourTime && virtualTimeHour < nightHourTime)
            {
                RenderSettings.skybox = daySkybox;

                if (virtualTimeHour > firstAuctionTime)
                {
                    DoAuction();
                }
            }
            
            
            // in the Night

            else
            {
                RenderSettings.skybox = nightSkybox;
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