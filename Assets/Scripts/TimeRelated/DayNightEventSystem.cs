using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TimeRelated;
using TMPro;
using UnityEngine.Serialization;
using VRRefAssist;

namespace TryScripts
{
    [Singleton]
    public class DayNightEventSystem : UdonSharpBehaviour
    {
        [Title("Day Night Duration Setting", "Unit: Hour; Check the Clock component below first.")]
        [PropertyRange(1, 50), Unit(Units.Hour)]
        [OnValueChanged("UpdateNightTimeDuration")]
        public int fullDayDuration = 24; // The total duration of a full day

        [PropertyRange(0, "$fullDayDuration"), Unit(Units.Hour)] [OnValueChanged("UpdateNightTimeDuration")]
        public int dayTimeDuration = 12; // The duration of the day

        [PropertyRange(0, "$fullDayDuration"), Unit(Units.Hour)] [OnValueChanged("UpdateDayTimeDuration")]
        public int nightTimeDuration = 12; // The duration of the night

        [PropertyRange(0, "$fullDayDuration"), InfoBox("$dayTimeSMInfo")]
        public int dayTimeStartMoment = 6;

        private int nightTimeStartMoment
        {
            get { return dayTimeDuration + dayTimeStartMoment; }
        }

        private string dayTimeSMInfo
        {
            get
            {
                return
                    $"Daytime starts at {dayTimeStartMoment}:00, and nighttime starts at {dayTimeStartMoment + dayTimeDuration}:00.";
            }
        }

        // Automatically adjusts nightTimeDuration when dayTimeDuration changes
        private void UpdateNightTimeDuration()
        {
            nightTimeDuration = Mathf.Clamp(fullDayDuration - dayTimeDuration, 0, fullDayDuration);
        }

        // Automatically adjusts dayTimeDuration when nightTimeDuration changes
        private void UpdateDayTimeDuration()
        {
            dayTimeDuration = Mathf.Clamp(fullDayDuration - nightTimeDuration, 0, fullDayDuration);
        }

        [Title("Exposure effect settings")] public Material skyboxMat;
        public float targetExposure; // 目标曝光值
        private readonly int _exposure = Shader.PropertyToID("_Exposure");
        //Assume the shader has the property called "_Exposure".


        [Title("Auction Related"), Required] 
        public TextMeshProUGUI auctionInfoUI;

        // public int firstAuctionTime;
        // public int secondAuctionTime;

        public UdonBehaviour auctionUdon;


        [Title("Auction Setting"), PropertyRange(1, 20)]
        public int numberOfAuctions = 5; // 白天需要进行的拍卖次数

        [Title("Doors")] public GameObject[] doors;

        private Collider _door1Collider;

        [Title("Audio Source")] public AudioSource audioSource;
        public AudioClip auctionCountdown;

        [TitleGroup("Readonly Data for runtime", "They don't actually updates...")] [ReadOnly]
        public Clock clockUdon; //clock got a Singleton, it would auto assigned!

        [ReadOnly] public bool alrSetDoorState;

        private bool _auctionInProgress = false;

        [TitleGroup("Readonly Data for runtime/Auction", boldTitle: false, horizontalLine: false, indent: false)]
        [ReadOnly]
        public int[] auctionMoments = { }; // 使用数组来存储每次拍卖的时间点

        [SerializeField, ReadOnly] private bool canAuction;

        [TitleGroup("Readonly Data for runtime/Time", boldTitle: false, horizontalLine: false, indent: false)]
        [Space, ReadOnly, SerializeField, InfoBox("If this reach a number in \"Auction Time\" above, auction starts.")]
        private int curTimeHourInGame;

        [ReadOnly] public bool isDay;

        private void Start()
        {
            alrSetDoorState = false;
            RenderSettings.skybox = skyboxMat;
            isDay = false;
            canAuction = true;

            var createAuctionMoments = new int[numberOfAuctions];
            auctionMoments = createAuctionMoments;

            CalculateAuctionTimes();

            if (skyboxMat != null)
            {
                SetExposure(targetExposure);
            }
        }

        private void SetExposure(float exposure)
        {
            // 假设曝光属性名为"_Exposure"，这需要根据你的Shader来确定
            if (skyboxMat.HasProperty(_exposure))
            {
                skyboxMat.SetFloat(_exposure, exposure);
            }
            else
            {
                Debug.LogError("Material does not have an '_Exposure' property.");
            }
        }


        private void CalculateAuctionTimes()
        {
            var auctionInterval = dayTimeDuration / (numberOfAuctions + 1);
            Debug.Log("AuctionInterval is " + auctionInterval);
            //var auctionInterval = (nightHourTime - dayHourTime) / (numberOfAuctions + 1); 
            //Note by Shengyang: Why night - day...?
            for (var i = 0; i < numberOfAuctions; i++)
            {
                var auctionTime = dayTimeDuration + auctionInterval * (i + 1);
                auctionMoments[i] = auctionTime;
                //  Debug.Log("Auction Moment " + i + " is " + auctionTime);
            }
        }

        private void Update()
        {
            if (skyboxMat)
            {
                SetExposure(targetExposure);
            }

            curTimeHourInGame = clockUdon.timeHourInGame; //= (int)_clockUdon.GetProgramVariable("timeHourInGame");

            targetExposure = 1.3f - Math.Abs(fullDayDuration / 2 - curTimeHourInGame) / 5.0f;
            //targetExposure = 1.3f - Math.Abs(12 - curTimeHourInGame) / 5.0f; //note by Shengyang: wth is this.....
            //sry but....wtffffffffff
            //write the freaking comments for a random number plzzzzzzzzzzzzzzz goshhhhhhhhhhhhhhhhh im freaking angry


            // 判断昼夜变化
            var wasDay = isDay;

            // ------------------ 在白天 ------------------
            if (dayTimeStartMoment < curTimeHourInGame && curTimeHourInGame < nightTimeStartMoment)
            {
                isDay = true;
            }
            // --------------- 在晚上 -----------------------
            else
            {
                isDay = false;
            }

            // 如果昼夜变化了，设置门的动画
            if (wasDay != isDay && !alrSetDoorState)
            {
                Debug.Log("Day Night shifts, now is day?: " + isDay);
                alrSetDoorState = true; // 标记为已经设置过门

                foreach (var door in doors)
                {
                    var unitDoor = door.GetComponent<UnitDoors>();
                    if (unitDoor && unitDoor.CanStartDayNight && unitDoor.anim)
                    {
                        unitDoor.anim.SetBool("Open", !isDay); // 白天关闭门
                        // 夜晚打开门
                    }
                }

                // 一旦设置完门的状态，恢复 setDoor 为 false，准备下次昼夜变化时再触发
                alrSetDoorState = false;

                if (isDay) // We come to the next day.
                {
                    clockUdon.ComeToNextDay();
                }
            }

            CheckAuction();
        }

        private void CheckAuction()
        {
            for (var i = 0; i < auctionMoments.Length; i++)
            {
                if (isDay && curTimeHourInGame == auctionMoments[i] && canAuction && !_auctionInProgress)
                {
                    // 开始拍卖
                    _auctionInProgress = true; // 标记拍卖进行中
                    audioSource.clip = auctionCountdown;
                    audioSource.Play();
                    auctionInfoUI.text = $"Auction No. {i + 1} Starts!";
                    canAuction = false; // 禁用拍卖开关

                    auctionUdon.SetProgramVariable("canDoAuction", true);
                    // ButtonActivated();

                    // 启动拍卖物品展示
                    auctionUdon.SendCustomEvent("DisplayAuctionItem");
                }
                else if (isDay && curTimeHourInGame > auctionMoments[i] && !canAuction)
                {
                    // 拍卖结束
                    _auctionInProgress = false; // 标记拍卖结束
                    canAuction = true; // 重新启用拍卖开关
                    auctionInfoUI.text = $"Auction No. {i + 1} Ends";
                    // ButtonDeActivated();
                }
            }

            if (!isDay)
            {
                auctionInfoUI.text = curTimeHourInGame.ToString() + ", There is no auction during the night.";
            }
        }
    }
}