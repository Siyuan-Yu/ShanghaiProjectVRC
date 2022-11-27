
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace TryScripts
{
    public class Clock : UdonSharpBehaviour
    {

        public Text curTimeInUI;
        public int timeRatio;
        [SerializeField] public int curTimeSecond;
        [SerializeField] public int curTimeMinute;
        [SerializeField] public int curTimeHour;
        [SerializeField] public string curTimeString;
        [SerializeField] public string curGameVirtualTimeString;

        [SerializeField] public int localTimeHour, localTimeMinute, localTimeSecond;


        // Start is called before the first frame update
        void Start()
        {
            
            // 有网的时候用这个测
            if (VRC.SDKBase.Networking.IsNetworkSettled)
            {
                var curtime = VRC.SDKBase.Networking.GetNetworkDateTime();
                
                curTimeSecond = curtime.Second;
                curTimeMinute = curtime.Minute;
                curTimeHour = curtime.Hour;
                curTimeString = curtime.ToString();
            }
            
            else
            {
                curTimeString = "Not Connected To Internet";
            }

            //没网的时候用这个测
            // var curtime = DateTime.Now;
            //
            // curTimeSecond = curtime.Second;
            // curTimeMinute = curtime.Minute;
            // curTimeHour = curtime.Hour;
            // curTimeString = curtime.ToString();
            //
        }

        // Update is called once per frame
        void Update()
        {
            //没网的时候用这个测
            // var curtime = System.DateTime.Now;
            
            //有网的时候连这个
            var curtime = VRC.SDKBase.Networking.GetNetworkDateTime();
            curTimeSecond = curtime.Second;
            curTimeMinute = curtime.Minute;
            curTimeHour = curtime.Hour;
            curTimeString = curtime.ToString();

            GameVirtualTime();
            
            curTimeInUI.text = "The Real Current Time Now is " + curTimeString + "\n" + "The Virtual Time is " + curGameVirtualTimeString;
        }


        //根据网络同步的真实时间，按照比例计算游戏时间
        void GameVirtualTime()
        {
            int localTotalTime = (curTimeHour % (24/timeRatio)) * 60 * 60 + (curTimeMinute) * 60 + curTimeSecond;
            localTimeHour = localTotalTime / (3600 / timeRatio);
            int localTotalTimeRemainAfterHour = localTotalTime % (3600 / timeRatio);
            localTimeMinute = localTotalTimeRemainAfterHour / (60 / timeRatio);
            int localTotalTimeRemainAfterMinute = localTotalTimeRemainAfterHour % (60 / timeRatio);
            localTimeSecond = localTotalTimeRemainAfterMinute * timeRatio;

            string localTimeHourString, localTimeMinuteString, localTimeSecondString;

            if (localTimeHour < 10)
            {
                localTimeHourString = "0" + localTimeHour.ToString();
            }
            else
            {
                localTimeHourString = localTimeHour.ToString();
            }

            if(localTimeMinute < 10)
            {
                localTimeMinuteString = "0" + localTimeMinute.ToString();
            }
            else
            {
                localTimeMinuteString = localTimeMinute.ToString();
            }
            
            if (localTimeSecond < 10)
            {
                localTimeSecondString = "0" + localTimeSecond.ToString();
            }
            else
            {
                localTimeSecondString = localTimeSecond.ToString();
            }

            curGameVirtualTimeString = localTimeHourString + " : " + localTimeMinuteString + " : " + localTimeSecondString;
            // curGameVirtualTimeString = localTimeHourString + " : 00 " + " : 00";
        }
    }

}