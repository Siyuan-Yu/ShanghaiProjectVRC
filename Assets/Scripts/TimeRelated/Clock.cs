
using System;
using System.Globalization;
using Auction;
using Sirenix.OdinInspector;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRRefAssist;

namespace TimeRelated
{
    [Singleton]
    public class Clock : UdonSharpBehaviour
    {
        [Title("Components")]
        public TextMeshProUGUI curTimeInUI;

        [Title("Setting")]
        [PropertyRange(1, 720)]
        //[InfoBox("It means: 1 real minute would be <i>X</i> minutes in the game. \n1 minute in game now is: (hour) and 1 day is: (min)"), InfoBox("$EachRealMinToVirtualHour",SdfIconType.Bell),InfoBox("$EachVirtualDayToRealMin",SdfIconType.Clock)]
        // [DetailedInfoBox("1 minute in game now is: (hour)","$timeRatioCalculated")]
        [InfoBox("$_timeRatioCalculateResult", "@dne")]
        [OnValueChanged("CalculateTimeRatioResult")]
        public int timeRatio = 180;

        [Space,
         InfoBox("You need to assign the DNE system to make the auto calculation work.", "@!dne")]
        public DayNightEventSystem dne;

        private string _timeRatioCalculateResult;

        [HideInInspector] public AuctionItemManager auctionItemManager;

        private void CalculateTimeRatioResult()
        {
            var eachRealMinToVirtualHour = timeRatio / 60f;
            var eachVirtualDayToRealMin = dne.fullDayDuration / eachRealMinToVirtualHour;
            _timeRatioCalculateResult =
                $"It means: 1 real minute would be {timeRatio} minutes in the game.\n<b>1 real minute in game now is: {eachRealMinToVirtualHour} hours; and 1 virtual day is: {eachVirtualDayToRealMin} real minutes</b>";
            
            //Info Communication:
            if(auctionItemManager)
                auctionItemManager.timeRatio = timeRatio;
        }
        //private float EachRealMinToVirtualHour { get { return timeRatio / 60f; } }
        //private float EachVirtualDayToRealMin { get { return 24f / EachRealMinToVirtualHour; } }

        [Title("DayCount", "Start From 0."), ReadOnly ,InfoBox("$dayCount")] [UdonSynced]
        public int dayCount = 0;

        /*[VRC.Udon.Serialization.OdinSerializer.OdinSerialize] // UdonSharp auto-upgrade: serialization 
        private DateTime _curTime;

        /*[Title("Current Real Time (Readonly)", "$curRealTimeHour")]//"They don't actually updates...")] 
        [ReadOnly]
        public int curRealTimeSecond;

        [ReadOnly] public int curRealTimeMinute;
        [ReadOnly][ShowInInspector] public int curRealTimeHour;
        [Space] [ReadOnly] public string curTimeString;*/

        [Title("Current Time in Game")] [ReadOnly] [UdonSynced]
        public int timeHourInGame;
        [ReadOnly] [UdonSynced] public int timeMinuteInGame;
        [ReadOnly] [UdonSynced] public int timeSecondInGame;
        [ReadOnly,HideInInspector] public string curGameVirtualTimeString;
        
        private float _virtualTimeElapsed = 0f;
        
        [Title("Values from DNE")]
        private int _fullDayDuration;
        private int _dayTimeDuration;
        private int _nightTimeDuration;
        private int _dayTimeStartMoment; //TODO: Change the start point and calc logic of virtual time convertion.
        private int _virtualStartMoment;

        [Title("Networking and sync")]
        private bool _isFirstPlayer = false; 
        
        
        private void Start()
        {
            _isFirstPlayer = Networking.IsOwner(gameObject);  // Check if this player is the owner

            if (_isFirstPlayer)
            {
                InitializeTime();
            }
        }
        
        private void Update()
        {
            // If we are the first player, update time
            if (_isFirstPlayer)
            {
                UpdateVirtualTime();
            }

            // Update time for all players
            UpdateDisplayTime();
        }
        
        private void UpdateVirtualTime()
        {
            // Update virtual time (use Time.deltaTime for smooth time progression)
            _virtualTimeElapsed += Time.deltaTime * timeRatio;
            
            int totalVirtualMinutes = Mathf.FloorToInt(_virtualTimeElapsed / 60);
            timeHourInGame = totalVirtualMinutes / 60;
            timeMinuteInGame = totalVirtualMinutes % 60;
            timeSecondInGame = Mathf.FloorToInt(_virtualTimeElapsed % 60);
            
            SyncTime();
        }
        
        private void InitializeTime()
        {
            _virtualStartMoment = dne.virtualStartMoment;
            
            timeHourInGame = _virtualStartMoment;
            timeMinuteInGame = 0;
            timeSecondInGame = 0;
            _virtualTimeElapsed = _virtualStartMoment * 3600f;
            
            Debug.Log("Initial start time elapsed: " + _virtualTimeElapsed);
            
            SyncTime();
        }

        private void UpdateDisplayTime()
        {
            curGameVirtualTimeString = $"{timeHourInGame:D2} : {timeMinuteInGame:D2}";
            curTimeInUI.text = "Time: " + curGameVirtualTimeString;
        }
        
        private void SyncTime()
        {
            if (_isFirstPlayer)
            {
                // Notify all players to update their time
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ReceiveTimeUpdate");
            }
        }
        
        
        public void ReceiveTimeUpdate()
        {
            // Sync the local time to match the host's time
            UpdateDisplayTime();
        }
        
        public void ComeToNextDay()
        {
            dayCount++;
            Debug.Log("Here we come to the next day: day " + dayCount);
        }

        private void OnValidate()
        {
            CalculateTimeRatioResult();
        }
        
        public override void OnDeserialization()
        {
            // Optionally update UI or other logic when synced time is received from other players
            if (!_isFirstPlayer)
            {
                UpdateDisplayTime();
            }
        }
        
        #region Original Method: Use Networking

        

        
        /*
        private void Start()
        {
            // 有网的时候用这个测
            if (Networking.IsNetworkSettled)
            {
                _curTime = Networking.GetNetworkDateTime();

                curRealTimeSecond = _curTime.Second;
                curRealTimeMinute = _curTime.Minute;
                curRealTimeHour = _curTime.Hour;
                curTimeString = _curTime.ToString(CultureInfo.CurrentCulture);
            }

            else
            {
                curTimeString = "Not Connected To Internet";
            }
            
            _fullDayDuration = dne.fullDayDuration;
            _dayTimeDuration = dne.dayTimeDuration;
            _nightTimeDuration = dne.nightTimeDuration;
            _dayTimeStartMoment = dne.dayTimeStartMoment;
            
            _virtualStartMoment = dne.dayTimeStartMoment;

            //没网的时候用这个测
            // curTime = DateTime.Now;
            //
            // curTimeSecond = curTime.Second;
            // curTimeMinute = curTime.Minute;
            // curTimeHour = curTime.Hour;
            // curTimeString = curTime.ToString();
            
            
        }

        private void Update()
        {
            //没网的时候用这个测
            // curTime = System.DateTime.Now;

            //有网的时候连这个
            _curTime = Networking.GetNetworkDateTime();
            curRealTimeSecond = _curTime.Second;
            curRealTimeMinute = _curTime.Minute;
            curRealTimeHour = _curTime.Hour;
            curTimeString = _curTime.ToString(CultureInfo.CurrentCulture);
            Debug.Log("curVirtualTimeHour" + timeHourInGame);
            GameVirtualTime();

            curTimeInUI.text = "The Real Current Time Now is " + curTimeString + "\n" + "The Virtual Time is " +
                               curGameVirtualTimeString;
        }


        // Calculate the in-game time proportionally based on real-time synchronized via the network
        private void GameVirtualTime()
        {
            if (timeRatio <= 60f)
            {
                // Multiply the real-time values by their respective unit seconds to get the total real-time seconds
                var realTotalTime = curRealTimeHour * 60 * 60 + curRealTimeMinute * 60 + curRealTimeSecond;
                // Calculate the total hours in the virtual time based on the timeRatio
                var localTotalHours = realTotalTime / (60 * 60 / timeRatio);
                // Calculate the total number of days passed in the current timeRatio (not used)
                var localTotalDays = localTotalHours / 24;
                // Calculate the current hour position in the virtual time
                timeHourInGame = localTotalHours - localTotalDays * 24; //TODO!!! Note by Shengyang: So... the starting point of the game is not fixed??
                // Remaining real-time seconds after subtracting all full virtual hours
                var realTotalTimeRemainAfterHour = realTotalTime - localTotalHours * (60 * 60 / timeRatio);

                timeMinuteInGame = realTotalTimeRemainAfterHour / (60 / timeRatio);

                var realTotalTimeRemainAfterMinute = realTotalTimeRemainAfterHour - timeMinuteInGame * (60 / timeRatio);
                timeSecondInGame = realTotalTimeRemainAfterMinute * timeRatio;
            }

            else
            {
                var localRatio = timeRatio / 60;

                var realTotalTime = curRealTimeHour * 60 * 60 + curRealTimeMinute * 60 + curRealTimeSecond;
                var localTotalHours = realTotalTime / (60 / localRatio);
                var localTotalDays = localTotalHours / 24;
                timeHourInGame = localTotalHours - localTotalDays * 24;
                var realTotalTimeRemainAfterHour = realTotalTime - localTotalHours * (60 / localRatio);
                timeMinuteInGame = realTotalTimeRemainAfterHour * localRatio;

                timeSecondInGame = 0;
            }


            string localTimeHourString, localTimeMinuteString, localTimeSecondString;

            if (timeHourInGame < 10)
                localTimeHourString = "0" + timeHourInGame.ToString();
            else
                localTimeHourString = timeHourInGame.ToString();


            if (timeMinuteInGame < 10)
            {
                localTimeMinuteString = "0" + timeMinuteInGame.ToString();
            }
            else
            {
                localTimeMinuteString = timeMinuteInGame.ToString();
            }

            if (timeSecondInGame < 10)
            {
                localTimeSecondString = "0" + timeSecondInGame.ToString();
            }
            else
            {
                localTimeSecondString = timeSecondInGame.ToString();
            }

            curGameVirtualTimeString =
                localTimeHourString + " : " + localTimeMinuteString; //+ " : " + localTimeSecondString;
            // curGameVirtualTimeString = localTimeHourString + " : 00 " + " : 00";
        }*/
        #endregion

        
    }
}