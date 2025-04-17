
using System;
using Auction;
using Sirenix.OdinInspector;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRRefAssist;
using Array = Utilities.Array;

namespace TimeRelated
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
        
        [InfoBox("Start Moment (Virtual Start of Game)"),PropertyRange(0, "$fullDayDuration")]
        public int virtualStartMoment = 6;
        
        [PropertyRange(0, "$fullDayDuration")][Title("When clock reaches this moment, it would play the Auction Prepare Announcement","It is better to set this before the first auction", bold: false, horizontalLine: false)]
        public int auctionPrepareAnnouncementMoment = 6;
        
        private int nightTimeStartMoment
        {
            get { return dayTimeDuration + dayTimeStartMoment; }
        }

        private string dayTimeSMInfo //Start moment for infobox
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
            nightTimeDuration = (int) Mathf.Clamp(fullDayDuration - dayTimeDuration, 0, fullDayDuration);
        }

        // Automatically adjusts dayTimeDuration when nightTimeDuration changes
        private void UpdateDayTimeDuration()
        {
            dayTimeDuration = (int)Mathf.Clamp(fullDayDuration - nightTimeDuration, 0, fullDayDuration);
        }

        [Title("Exposure effect settings")] public Material skyboxMat;
        [Range(0, 1.3f)] public float nightExposure = 0.02f;
        [Range(0, 3f)] public float nightAtmosphereThickness = 1.2f;
        [Range(0, 1.3f)] public float daytimeMaxExposure = 1.3f;
        [Range(0, 1.3f)] public float daytimeAtmosphereThickness = 1.2f;
        [HideInInspector]public float targetExposure; // 目标曝光值
        private readonly int _exposure = Shader.PropertyToID("_Exposure");
        private readonly int _atmosphereThickness = Shader.PropertyToID("_AtmosphereThickness");
        //Assume the shader has the property called "_Exposure".


        [Title("Auction Related"), Required] 
        public TextMeshProUGUI auctionInfoUI;

        // public int firstAuctionTime;
        // public int secondAuctionTime;

        public AuctionItemManager auctionManager;


        [Title("Auction Setting"), PropertyRange(1, 20), InfoBox("$_auctionMomentsInfo"),OnValueChanged("OnNumberOfAuctionsChanged")]
        public int numberOfAuctions = 5; // 白天需要进行的拍卖次数
        
        [TitleGroup("Readonly Data for runtime/Auction", boldTitle: false, horizontalLine: false, indent: false)]
        [ReadOnly]
        public int[] auctionMoments = { }; // 使用数组来存储每次拍卖的时间点

        //[SerializeField, ReadOnly] private bool canAuction;
        private bool canStartAuction = true;
        
        private bool _auctionInProgress = false;

        #region Inspector

        private string _auctionMomentsInfo = "Try Changing the number of Auctions to start calculation";

        private void OnNumberOfAuctionsChanged()
        {
            CalculateAuctionTimes();
            _auctionMomentsInfo = "Auction Moments are: ";
            foreach (var moment in auctionMoments)
            {
                _auctionMomentsInfo += moment + ":00, ";
            }
        }

        #endregion

        [Title("Doors")] public GameObject[] doors;

        private Collider _door1Collider;

        [Title("Audio Source")] public AudioSource audioSource;
        public AudioClip auctionCountdown;

        [TitleGroup("Readonly Data for runtime", "They don't actually updates...")] [ReadOnly]
        public Clock clockUdon; //clock got a Singleton, it would auto assigned!

        [ReadOnly] public bool alrSetDoorState;
        
        
        [TitleGroup("Readonly Data for runtime/Time", boldTitle: false, horizontalLine: false, indent: false)]
        [Space, ReadOnly, SerializeField, InfoBox("If this reach a number in \"Auction Time\" above, auction starts.")]
        private int _curTimeHourInGame;
        public int curTimeHourInGame
        {
            get { return _curTimeHourInGame; }
            set
            {
                if (value == curTimeHourInGame) return;
                
                _curTimeHourInGame = value;
                
                if (value == auctionPrepareAnnouncementMoment)
                {
                    auctionManager.PlayAuctionStartAnnouncement();
                }
                
                if (!isDay) return; // because auction only happens during daytime
                
                if (Array.IntContains(auctionMoments, curTimeHourInGame))
                {
                    //audioSource.clip = auctionCountdown;
                    if (debugMode)
                        Debug.Log("Auction Starts!");
                    auctionManager.SetCanAuction(true);
                    //auctionManager.SetProgramVariable("canDoAuction",true);
                }
                else
                {
                    if(debugMode)
                        Debug.Log("Not an Auction hour");
                }

                

            }
        }

        [ReadOnly] public bool isDay;

        [Title("Debug")] public bool debugMode = false;

        private void Start()
        {
            alrSetDoorState = false;
            RenderSettings.skybox = skyboxMat;
            isDay = false;

            CalculateAuctionTimes();

            /*if (skyboxMat != null)
            {
                SetExposure(targetExposure);
            }*/
        }

        private void SetExposure(float exposure)
        {
            // 假设曝光属性名为"_Exposure"，这需要根据你的Shader来确定
            if (skyboxMat.HasProperty(_exposure))
            {
                skyboxMat.SetFloat(_exposure, exposure);
                Debug.Log("Skybox mat exposure set to " + exposure);
            }
            else
            {
                Debug.LogError("Material does not have an '_Exposure' property.");
            }
        }

        private void SetExposure()
        {
            if (!skyboxMat)
            {
                Debug.LogError("There is no skybox mat assigned in DNE");
                return;
            }
            if (curTimeHourInGame <= dayTimeStartMoment || curTimeHourInGame >= nightTimeStartMoment)
            {
                // during the night
                skyboxMat.SetFloat(_exposure, nightExposure);
                skyboxMat.SetFloat(_atmosphereThickness, nightAtmosphereThickness);
            }

            else
            {

                var dayProgress = (float)(curTimeHourInGame - dayTimeStartMoment) /
                                  (nightTimeStartMoment - dayTimeStartMoment);
                dayProgress = Mathf.Clamp01(dayProgress); 
                
                var exposure = Mathf.Lerp(nightExposure, daytimeMaxExposure, dayProgress);
                skyboxMat.SetFloat(_exposure, exposure);
                
                /*var atmosphereThickness = Mathf.Lerp(nightAtmosphereThickness, daytimeAtmosphereThickness, dayProgress);
                skyboxMat.SetFloat(_atmosphereThickness, atmosphereThickness);*/
                skyboxMat.SetFloat(_atmosphereThickness, daytimeAtmosphereThickness);
            }
        }


        private void CalculateAuctionTimes()
        {
            var createAuctionMoments = new int[numberOfAuctions];
            auctionMoments = createAuctionMoments;
            
            var auctionInterval = dayTimeDuration / (numberOfAuctions + 1);
            if(debugMode)
                Debug.Log("AuctionInterval is " + auctionInterval);
            //var auctionInterval = (nightHourTime - dayHourTime) / (numberOfAuctions + 1); 
            //Note by Shengyang: Why night - day...?
            var debugString = "";
            for (var i = 0; i < numberOfAuctions; i++)
            {
                var auctionTime = dayTimeStartMoment + auctionInterval * (i + 1);
                auctionMoments[i] = auctionTime;
                //  Debug.Log("Auction Moment " + i + " is " + auctionTime);
                debugString += auctionTime + ", ";
            }
            if(debugMode)
                Debug.Log("Calculated AuctionTimes are " + debugString);

            /*if (curTimeHourInGame < dayTimeStartMoment || curTimeHourInGame > nightTimeStartMoment)
            {
                alrSetDoorState = true; // 标记为已经设置过门

                foreach (var door in doors)
                {
                    var unitDoor = door.GetComponent<UnitDoors>();
                    if (unitDoor  && unitDoor.animator)
                    {
                        unitDoor.CloseDoor();
                    }
                }

                // 一旦设置完门的状态，恢复 setDoor 为 false，准备下次昼夜变化时再触发
                alrSetDoorState = false;
            }*/
        }
        
        public void OnDayChanged(int newDayCount)
        {
            // Reset auction items for the new day
            if (auctionManager)
            {
                if (Networking.IsOwner(auctionManager.gameObject))
                {
                    auctionManager.ResetAuctionedItems();
                }
            }
    
            // Any other day change logic
            Debug.Log("DayNightEventSystem: New day started: " + newDayCount);
        }

        private void Update()
        {
            if (skyboxMat)
            {
                SetExposure();
            }

            // Get current hour directly from clock
            curTimeHourInGame = clockUdon.timeHourInGame;

            // Calculate target exposure
            targetExposure = daytimeMaxExposure - Math.Abs(fullDayDuration / 2f - curTimeHourInGame) / 5.0f;
    
            // Check day/night status
            bool wasDay = isDay;
            isDay = (dayTimeStartMoment < curTimeHourInGame && curTimeHourInGame < nightTimeStartMoment);
    
            // Handle day/night transition
            if (wasDay != isDay && !alrSetDoorState)
            {
                HandleDayNightTransition();
            }
        }

        private void HandleDayNightTransition()
        {
            if(debugMode)
                Debug.Log("Day Night shifts, now is day?: " + isDay);
    
            alrSetDoorState = true; // Mark as processed
    
            // Update doors
            foreach (var door in doors)
            {
                var unitDoor = door.GetComponent<UnitDoors>();
                if (unitDoor && unitDoor.animator)
                {
                    unitDoor.ChangeDoorState();
                }
            }
    
            // Reset flag
            alrSetDoorState = false;
        }

        [Obsolete]
        private void CheckAuction()
        {
            if(isDay)
            {
                if (Array.IntContains(auctionMoments, curTimeHourInGame))
                {
                    if (canStartAuction && !_auctionInProgress)
                    {
                        // 开始拍卖
                        _auctionInProgress = true; // 标记拍卖进行中
                        audioSource.clip = auctionCountdown;
                        audioSource.Play();
                        auctionInfoUI.text = "Auction Starts!";//$"Auction No. {i + 1} Starts!";
                        if(debugMode)
                            Debug.Log("Auction Starts!");
                        canStartAuction = false; // 禁用拍卖开关

                        auctionManager.canDoAuction = true;
                        // ButtonActivated();

                        // 启动拍卖物品展示
                        auctionManager.DisplayAuctionItem();
                    }
                    else if(!canStartAuction)
                    {
                        // 拍卖结束
                        _auctionInProgress = false; // 标记拍卖结束
                        canStartAuction = true; // 重新启用拍卖开关
                        auctionInfoUI.text = "Auction Ends!"; //$"Auction No. {i + 1} Ends";
                        if(debugMode)
                            Debug.Log("Auction Ends!");
                        // ButtonDeActivated();
                    }
                }
                else
                {
                    auctionInfoUI.text = curTimeHourInGame + ", There is no auction at this moment.";
                }
            }
            {
                auctionInfoUI.text = curTimeHourInGame + ", There is no auction during the night.";
            }
        }
    }
}