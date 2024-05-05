
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
        
        [VRC.Udon.Serialization.OdinSerializer.OdinSerialize] /* UdonSharp auto-upgrade: serialization */ public DateTime curTime;
        
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
                curTime = VRC.SDKBase.Networking.GetNetworkDateTime();
                
                curTimeSecond = curTime.Second;
                curTimeMinute = curTime.Minute;
                curTimeHour = curTime.Hour;
                curTimeString = curTime.ToString();
            }
            
            else
            {
                curTimeString = "Not Connected To Internet";
            }

            //没网的时候用这个测
            // curTime = DateTime.Now;
            //
            // curTimeSecond = curTime.Second;
            // curTimeMinute = curTime.Minute;
            // curTimeHour = curTime.Hour;
            // curTimeString = curTime.ToString();
            
        }

        // Update is called once per frame
        void Update()
        {
            //没网的时候用这个测
            // curTime = System.DateTime.Now;
            
            //有网的时候连这个
            curTime = VRC.SDKBase.Networking.GetNetworkDateTime();
            curTimeSecond = curTime.Second;
            curTimeMinute = curTime.Minute;
            curTimeHour = curTime.Hour;
            curTimeString = curTime.ToString();

            GameVirtualTime();
            
            curTimeInUI.text = "The Real Current Time Now is " + curTimeString + "\n" + "The Virtual Time is " + curGameVirtualTimeString;
        }


        //根据网络同步的真实时间，按照比例计算游戏时间
        void GameVirtualTime()
        {
 
            
            if (timeRatio <= 60f)
            {  
                //真实的时间所有数字乘以对应单位应有的秒数，得到的总和
                int realTotalTime = curTimeHour * 60 * 60 + curTimeMinute * 60 + curTimeSecond;
                //根据timeRatio换算过后的总小时数量
                int localTotalHours = realTotalTime / (60 * 60 / timeRatio);
                //在当前timeRatio下过去的天数（不使用）
                int localTotalDays = localTotalHours / 24;
                //当前换算后的虚拟时间的时针应在的地方
                localTimeHour = localTotalHours - localTotalDays * 24;
                //减掉所有小时过后还剩下的真实时间的秒数
                int realTotalTimeRemainAfterHour = realTotalTime - localTotalHours * (60 * 60 / timeRatio);
                
                localTimeMinute = realTotalTimeRemainAfterHour / (60 / timeRatio);
                
                int realTotalTimeRemainAfterMinute = realTotalTimeRemainAfterHour - localTimeMinute * (60 / timeRatio);
                localTimeSecond = realTotalTimeRemainAfterMinute * timeRatio;
            }

            else
            {
                
                int localRatio = timeRatio / 60;
                
                int realTotalTime = curTimeHour * 60 * 60 + curTimeMinute * 60 + curTimeSecond;
                int localTotalHours = realTotalTime / (60 / localRatio);
                int localTotalDays = localTotalHours / 24;
                localTimeHour = localTotalHours - localTotalDays * 24;
                int realTotalTimeRemainAfterHour = realTotalTime - localTotalHours * (60 / localRatio);
                localTimeMinute = realTotalTimeRemainAfterHour * localRatio;
                
                localTimeSecond = 0;
            }
            

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