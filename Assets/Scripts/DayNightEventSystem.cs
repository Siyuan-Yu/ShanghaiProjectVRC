﻿using TryScripts;
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

        public Material skyboxMat;
        public float targetExposure;  // 目标曝光值
        
        public UdonBehaviour clockUdon;

        [Header("和拍卖相关")]

        public Text auctionInfoUI;

        // public int firstAuctionTime;
        // public int secondAuctionTime;

        public UdonBehaviour auctionUdon;
        public bool canAuction;
        
        [Header("拍卖配置")]
        public int numberOfAuctions; // 白天需要进行的拍卖次数
        public int[] auctionTimes; // 使用数组来存储每次拍卖的时间点

        //public UdonSharpBehaviour[] doors;
        public GameObject[] doors;

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
            RenderSettings.skybox = skyboxMat;
            isDay = false;
            canAuction = true;
            
            // door1.GetComponent<MeshRenderer>().enabled = false;
            // door2.GetComponent<MeshRenderer>().enabled = false;
            
            // _btn1.SetActive(false);
            
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
            // ------------------ in the Day ------------------
            if (virtualTimeHour > dayHourTime && virtualTimeHour < nightHourTime)
            {
                isDay = true;

                foreach (var door in doors)
                {
                    if (door.GetComponent<UnitDoors>().CanStartDayNight)
                    {
                        MeshRenderer[] meshRenderers = door.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in meshRenderers)
                        {
                            renderer.enabled = true;
                        }
                    
                        BoxCollider[] boxColliders = door.GetComponentsInChildren<BoxCollider>();
                        foreach (BoxCollider boxCollider in boxColliders)
                        {
                            boxCollider.isTrigger = false;
                        }
                    }
                }
            }
            // ---------------  in the Night -----------------------
            else
            {
                isDay = false;
                foreach (var door in doors)
                {
                    if (door.GetComponent<UnitDoors>().CanStartDayNight)
                    {
                        MeshRenderer[] meshRenderers = door.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in meshRenderers)
                        {
                            renderer.enabled = false;
                        }
                    
                        BoxCollider[] boxColliders = door.GetComponentsInChildren<BoxCollider>();
                        foreach (BoxCollider boxCollider in boxColliders)
                        {
                            boxCollider.isTrigger = true;
                        }
                    } 
                }
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