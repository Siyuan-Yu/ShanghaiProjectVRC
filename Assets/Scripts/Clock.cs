
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
        [SerializeField] public int curTimeSecond;
        [SerializeField] public int curTimeMinute;
        [SerializeField] public int curTimeHour;
        [SerializeField] public string curTimeString;

        // Start is called before the first frame update
        void Start()
        {
            // var curtime = (System.DateTime.Now - System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now));
            //  var curtime;

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
                 curTimeString = "fjsiodgwsd";
             }

            // curTimeString = VRC.SDKBase.Networking.IsNetworkSettled.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            curTimeInUI.text = curTimeString;
            //var curtime = (System.DateTime.Now - System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now));
            var curtime = VRC.SDKBase.Networking.GetNetworkDateTime();
            curTimeSecond = curtime.Second;
            curTimeMinute = curtime.Minute;
            curTimeHour = curtime.Hour;
            curTimeString = curtime.ToString();
        }
    }
}