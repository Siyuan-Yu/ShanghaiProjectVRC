using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace TryScripts
{
    public class DayNightEventSystem : UdonSharpBehaviour
    {
        public int virtualTimeHour;

        [Title("Day Night Setting")] 
        public bool setDoor;
        
        public bool isDay;

        public int dayHourTime;
        public int nightHourTime;

        public Material skyboxMat;
        public float targetExposure;  // 目标曝光值
        
        public UdonBehaviour clockUdon;

        [Header("Auction Related")]

        public Text auctionInfoUI;

        // public int firstAuctionTime;
        // public int secondAuctionTime;

        public UdonBehaviour auctionUdon;
        public bool canAuction;
        
        [Header("Auction Setting")]
        public int numberOfAuctions; // 白天需要进行的拍卖次数
        public int[] auctionTimes; // 使用数组来存储每次拍卖的时间点
        public GameObject[] doors;

        Collider _door1Collider;
        
        [Header("Audio Source")] 
        public AudioSource audioSource;
        public AudioClip auctionCountdown;
        
        void Start()
        {
            setDoor = false;
            RenderSettings.skybox = skyboxMat;
            isDay = false;
            canAuction = true;

            CalculateAuctionTimes();
            
            if (skyboxMat != null)
            {
                SetExposure(targetExposure);
            }
        }
        
        public void SetExposure(float exposure)
        {
            // 假设曝光属性名为"_Exposure"，这需要根据你的Shader来确定
            if (skyboxMat.HasProperty("_Exposure"))
            {
                skyboxMat.SetFloat("_Exposure", exposure);
            }
            else
            {
                Debug.LogError("Material does not have an '_Exposure' property.");
            }
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
            targetExposure = 1.3f - ((float)Math.Abs(12 - virtualTimeHour) / 5.0f);
             
            if (skyboxMat != null)
            {
                SetExposure(targetExposure);
            }
            
            virtualTimeHour = (int)clockUdon.GetProgramVariable("localTimeHour");
            
            // 判断昼夜变化
            bool wasDay = isDay;
        
            // ------------------ 在白天 ------------------
            if (virtualTimeHour > dayHourTime && virtualTimeHour < nightHourTime)
            {
                isDay = true;
            }
            // --------------- 在晚上 -----------------------
            else
            {
                isDay = false;
            }

            // 如果昼夜变化了，设置门的动画
            if (wasDay != isDay && !setDoor)
            {
                setDoor = true;  // 标记为已经设置过门

                foreach (var door in doors)
                {
                    if (door.GetComponent<UnitDoors>().CanStartDayNight)
                    {
                        if (door.GetComponent<UnitDoors>().anim != null)
                        {
                            if (isDay)
                            {
                                door.GetComponent<UnitDoors>().anim.SetBool("Open", false); // 白天关闭门
                            }
                            else
                            {
                                door.GetComponent<UnitDoors>().anim.SetBool("Open", true);  // 夜晚打开门
                            }
                        }
                    }
                }

                // 一旦设置完门的状态，恢复 setDoor 为 false，准备下次昼夜变化时再触发
                setDoor = false;
            }

            CheckAuction();
        }

        bool auctionInProgress = false;

        void CheckAuction()
        {
            for (int i = 0; i < auctionTimes.Length; i++)
            {
                if (isDay && virtualTimeHour == auctionTimes[i] && canAuction && !auctionInProgress)
                {
                    // 开始拍卖
                    auctionInProgress = true; // 标记拍卖进行中
                    audioSource.clip = auctionCountdown;
                    audioSource.Play();
                    auctionInfoUI.text = $"拍卖 {i + 1} 开始！";
                    canAuction = false; // 禁用拍卖开关

                    auctionUdon.SetProgramVariable("canDoAuction", true);
                    // ButtonActivated();
        
                    // 启动拍卖物品展示
                    auctionUdon.SendCustomEvent("DisplayAuctionItem");
                }
                else if (isDay && virtualTimeHour > auctionTimes[i] && !canAuction)
                {
                    // 拍卖结束
                    auctionInProgress = false; // 标记拍卖结束
                    canAuction = true; // 重新启用拍卖开关
                    auctionInfoUI.text = $"拍卖 {i + 1} 结束";
                    // ButtonDeActivated();
                }
            }

            if (!isDay)
            {
                auctionInfoUI.text = virtualTimeHour.ToString() + ", 现在是晚上没有拍卖";
            }
        }
    }
}