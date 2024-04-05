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

        // public int firstAuctionTime;
        // public int secondAuctionTime;

        public UdonBehaviour auctionUdon;
        public bool canAuction;
        
        [Header("拍卖配置")]
        public int numberOfAuctions; // 白天需要进行的拍卖次数
        private int[] auctionTimes; // 使用数组来存储每次拍卖的时间点

        //public UdonSharpBehaviour[] doors;
        public GameObject door1;
        Collider _door1Collider;

        public GameObject _btn1;
        public GameObject _btn2;
        public GameObject _btn3;
        public GameObject _btn4;

        // public UdonBehaviour pointSystem;
        // public int localPoint;

        // public Clock clockUdon;
        [Header("音频文件")] 
        public AudioSource audioSource;
        public AudioClip auctionCountdown;
        
        void Start()
        {
            isDay = false;
            canAuction = true;

            door1.SetActive(false);
            // virtualTimeHour = clockUdon.localTimeHour;

            //不知道怎么udon findgameobjectswithtag
            // doors = GameObject.FindGameObjectsWithTag("rm_door");

            //
            _btn1.SetActive(false);
            // _btn2.SetActive(false);
            // _btn3.SetActive(false);
            // _btn4.SetActive(false);

            auctionTimes = new int[numberOfAuctions];
            CalculateAuctionTimes();
        }
        
        void CalculateAuctionTimes()
        {
            int auctionInterval = (nightHourTime - dayHourTime) / (numberOfAuctions + 1);
            for (int i = 0; i < numberOfAuctions; i++)
            {
                int auctionTime = dayHourTime + auctionInterval * (i + 1);
                auctionTimes[i] = auctionTime;
            }
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
            for (int i = 0; i < auctionTimes.Length; i++)
            {
                if (isDay && virtualTimeHour == auctionTimes[i] && canAuction)
                {
                    audioSource.clip = auctionCountdown;
                    audioSource.Play();
                    auctionInfoUI.text = $"拍卖 {i + 1} 开始！";
                    canAuction = false;
                    auctionUdon.SetProgramVariable("canDoAuction", true);
                    ButtonActivated();
                }
                else if (isDay && virtualTimeHour > auctionTimes[i])
                {
                    canAuction = true;
                    auctionInfoUI.text = $"拍卖 {i + 1} 结束";
                    ButtonDeActivated();
                }
            }

            if (!isDay)
            {
                auctionInfoUI.text = virtualTimeHour.ToString() + ", 现在是晚上没有拍卖";
            }
        }

        void ButtonActivated()
        {
            // localPoint = (int)pointSystem.GetProgramVariable("points");
            // localPoint += 2;
            // pointSystem.SetProgramVariable("points", localPoint);
            _btn1.SetActive(true);
            // _btn2.SetActive(true);
            // _btn3.SetActive(true);
            // _btn4.SetActive(true);
        }

        void ButtonDeActivated()
        {
            _btn1.SetActive(false);
            // pointSystem.SetProgramVariable("point", 0);
            // _btn2.SetActive(false);
            // _btn3.SetActive(false);
            // _btn4.SetActive(false);
        }
    }
}