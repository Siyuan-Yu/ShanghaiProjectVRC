﻿using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;


namespace UdonSharp.Examples.Tutorials
{
    public class ClockValue : UdonSharpBehaviour
    {
        public Text curTimeInUI;
        [SerializeField] public int curTimeSecond;
        [SerializeField] public int curTimeMinute;
        [SerializeField] public int curTimeHour;
        [SerializeField] public string curTimeString;

        // Start is called before the first frame update
        void Start()
        {
            var curtime = (System.DateTime.Now - System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now));
            curTimeSecond = curtime.Second;
            curTimeMinute = curtime.Minute;
            curTimeHour = curtime.Hour;
            curTimeString = curtime.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            curTimeInUI.text = curTimeString;
            var curtime = (System.DateTime.Now - System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now));
            curTimeSecond = curtime.Second;
            curTimeMinute = curtime.Minute;
            curTimeHour = curtime.Hour;
            curTimeString = curtime.ToString();
        }
    }
}
