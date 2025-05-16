
using System;
using System.Globalization;
using System.Net;
using Inventory;
using NukoTween;
using Sirenix.OdinInspector;
using TMPro;
using UdonSharp;
using VRRefAssist;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;
using VRC.SDKBase;
using VRC.Udon;
using Array = Utilities.Array;
using Random = UnityEngine.Random;

namespace Auction
{
    [Singleton, UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AuctionItemManager : UdonSharpBehaviour
    {
        [Title("Auction Settings"),ReadOnly,Searchable] public GameObject[] auctionItems;
        
        [ReadOnly,Searchable] public AuctionSample[] auctionSamples;
        [Range(1,50)]public uint itemPoolSize = 10;

        #region debug

        private int _previousLength;

        private void CheckLength()
        {
            if (_previousLength != auctionItems.Length)
            {
                if(debugMode)
                    Debug.Log("Auction Manager: new auction items length: " + _previousLength + " to " + auctionItems.Length);
                _previousLength = auctionItems.Length;
            }
        }

        #endregion
        
        private GameObject[] auctionedItemsToday;
        private GameObject _selectedItemToAuction;
        [UdonSynced] private int _selectedItemToAuctionIndex  = -1;
        private AuctionSample _selectedSampleToAuctionComponent;
        private AuctionButton _winnerThisRound;
        
       // [InfoBox()]
        [SerializeField, Unit(Units.Second), InfoBox("$_auctionDurationLengthConvert"),OnValueChanged("_auctionDurationLengthConverter")] 
        private float auctionDuration = 15f;
        [InfoBox("When displaying an item, the object would rotate until it is bought or expired"),Unit(Units.DegreesPerSecond)]
        public float auctionItemRotateYSpeed;
        
        [ReadOnly,UdonSynced] public bool canDoAuction; // controlled by DNE System
        //[UdonSynced] 
        private bool _isDisplayingItem;
        [UdonSynced] private float _auctionDisplayStartTime;
        #region Inspector

        private string _auctionDurationLengthConvert = "Try Changing the value to start calculation.";
        [HideInInspector] public int timeRatio; 

        private void _auctionDurationLengthConverter()
        {
            if (timeRatio != 0)
                _auctionDurationLengthConvert = $"Currently each auction would last: {auctionDuration} real seconds and {(auctionDuration * (timeRatio / 60f)).ToString(CultureInfo.CurrentCulture)} virtual minutes";
            else
                _auctionDurationLengthConvert = "Currently failed to get the timeRatio from Clock.cs, try running the game first.";
        }

        #endregion
        
        

       // [InfoBox("When one is unsold, the time to wait to switch to next auction item"),Unit(Units.Second)]
       // public float switchToNextAuctionTime = 3f;

        //private bool alrUnsoldOnce;
        
        [Title("Unit Management")]
        [SerializeField] private GameObject allUnitsContainer;
        [SerializeField,ReadOnly,Searchable] private AuctionButton[] auctionButtons;
        /*private GameObject[] validUnits;
        [SerializeField, ReadOnly] private int validUnitCount = 0;*/

        [Title("UI Elements")]
        [SerializeField] private TextMeshProUGUI auctionWinnerInfoUI;
        [SerializeField] private TextMeshProUGUI auctionInfoUI;
        
        [TitleGroup("Audio Settings")]
        [SerializeField,Required] private AudioSource auctionAudioSource;

        [TitleGroup("Audio Settings/Prepare Notification")] [InfoBox("When the time reaches the moment set in the DNE Sys, the clip would be played.")]
        [SerializeField,Required] private AudioClip auctionPrepareAnnouncementAudioClip;
       // [TitleGroup("Audio Settings/Start Notification")] [SerializeField,Unit(Units.Second)] private float timeToWaitFromStartNotif = 20f;
        
        [TitleGroup("Audio Settings/Countdown")]
        [SerializeField] private AudioClip auctionCountdown;
        [TitleGroup("Audio Settings/Countdown")] [SerializeField] private float countdownAudioDuration = 3f;
        private bool _playedCountdownThisTime = false;

        [TitleGroup("Delivery Settings"),Unit(Units.Meter)]
        /*[SerializeField] private GameObject defaultDeliverPlace;
        [SerializeField] private float smallItemScaleMultiplier = 0.05f;*/
        public float deliveryYOffset = 1;
        [TitleGroup("Delivery Settings"),Unit(Units.Second)]
        public float deliveryFlyDuration = 5f;
        
        [TitleGroup("Delivery Settings/Tweens"),ReadOnly] 
        public TweenManager tweenManager;

        private int _rotateTweenId;
        private int _flyToPlayerTweenId;
        /*[TitleGroup("Delivery Settings/Rotating")]
        public float deliveryRotateYSpeed;*/

        [Title("Global Gameplay Settings")] 
        [HideInInspector]public PointSystem pointSystem;
        public bool canAddPoint = true;
        public int addPointVal = 10;

        [Title("Each Auction Data")] 
        [HideInInspector] public int unitIndexToGo;

        [Title("Setting"), SerializeField] private bool debugMode;
        [HideInInspector] public bool gameIsRunning;
        
        [SerializeField] private TextMeshProUGUI auctionDebugText;

        private void Awake()
        {
            
            if (!Networking.IsOwner(gameObject)) return;
            
            if(debugMode) Debug.Log("Auction Manager awake and reset arrays");

            gameIsRunning = true;

            Debug.Log("Manager reached 1");
        }

        private void Start() //In the Script Execution Order, Item is earlier than Manager.
        {
            auctionedItemsToday = new GameObject[0];
            
            if(! auctionAudioSource) Debug.LogError("Missing Audio Source on " + gameObject.name);
            //InitializeUnits();
            //InitializeAuctionItems();
            //SendCustomEventDelayedSeconds(nameof(InitializeAuctionItems), 1f);
            
        }

        [Button("2-Collect All Samples")]
        private void GetItemSamples()
        {
            if (gameIsRunning)
            {
                Debug.LogWarning("Do not use GetItemSamples in play mode, aborting");
                return;
            }
            auctionSamples = GetComponentsInChildren<AuctionSample>();
            auctionItems = new GameObject[auctionSamples.Length];

            for (int i = 0; i < auctionSamples.Length; i++)
            {
                auctionItems[i] = auctionSamples[i].gameObject;
                auctionSamples[i].GetRenderers();
                auctionItems[i].GetComponent<Item>().enabled = false;
            }
        }

        [Button("1-Clean all Items")]
        private void CleanAllItemInstances()
        {
            foreach (var sample in auctionSamples)
            {
                sample.ClearInstances();
            }
        }

        [Button("3-Create Instances for all objects")]
        private void GenerateInstances()
        {
            foreach (var sample in auctionSamples)
            {
                sample.GenerateInstances();
            }
        }
        

        private void Update()
        {
            /*if (debugMode)
                CheckLength();*/
            /*if(debugMode)
                Debug.Log("Can Auction: " + canDoAuction + " " + "is Displaying Item: " + _isDisplayingItem);*/
            if(!Networking.IsOwner(gameObject)) return;
            
            if (canDoAuction && !_isDisplayingItem)
            {
                DisplayAuctionItem();
                canDoAuction = false; // 确保只调用一次
            }
            
            //--------------------------------
            if (_isDisplayingItem)
            {
                var timePassed = Time.time - _auctionDisplayStartTime;
                if (timePassed >= auctionDuration - countdownAudioDuration && !_playedCountdownThisTime)
                {
                    auctionAudioSource.PlayOneShot(auctionCountdown);
                    _playedCountdownThisTime = true;
                }
                else if(timePassed >= auctionDuration)
                {
                    _isDisplayingItem = false;
                    _winnerThisRound = GetAuctionWinner();
                   // tween.Kill(_rotateTweenId);
                    if (_winnerThisRound)
                    {
                        /*var generatedItem = Instantiate(_selectedItemToAuction, _selectedItemToAuction.transform.position, _selectedItemToAuction.transform.rotation);
                        _selectedItemToAuction.SetActive(false);
                        generatedItem.name = _selectedItemToAuction.name;
                        selectedSampleToAuctionComponent.isAuctionedToday = true;
                        var generatedItemComponent = generatedItem.GetComponent<AuctionSample>();
                        
                        generatedItemComponent.StartFlyingToPlayer(_winnerThisRound.transform);*/
                        //TODO
                        _selectedSampleToAuctionComponent.StartFlyingToPlayer(_winnerThisRound.transform);
                        Debug.Log("There is winner and it is " + _winnerThisRound.name + " " + _winnerThisRound.playerName);
                        if(_winnerThisRound.playerName == "")
                        {
                            if (auctionWinnerInfoUI)
                                auctionWinnerInfoUI.text = "Winner: " + _winnerThisRound.name;
                        }
                        else
                            if(auctionWinnerInfoUI)
                                auctionWinnerInfoUI.text = "Winner: " + _winnerThisRound.playerName;
                    }
                    else
                    {
                        _selectedItemToAuction.SetActive(false);
                        Debug.Log("There is no winner in this round.");
                    }

                    _selectedSampleToAuctionComponent.SwitchRenderers();

                    _selectedItemToAuction = null;
                    _selectedSampleToAuctionComponent = null;
                    _selectedItemToAuctionIndex = -1;
                    RequestSerialization();
                }
                else
                {
                    if (auctionWinnerInfoUI)
                        auctionWinnerInfoUI.text = "Auction is going on for " + (auctionDuration - timePassed).ToString("0.0") + " seconds";
                }
            }
            
        }

        public void DisplayAuctionItem()
        {
            Debug.Log("Auction: Try Start Displaying.");
            if(auctionDebugText)
            {
                auctionDebugText.text = $"Auction: Try Start Displaying: {_selectedItemToAuctionIndex}";
                auctionDebugText.color = Color.red;
            }
            if(Networking.IsOwner(gameObject))
            {
                _winnerThisRound = null;
                ResetAllUnitClickCounts();

                _selectedItemToAuction = GetRandomAuctionItem();
            }
            else
            {
                if(_selectedItemToAuctionIndex == -1) return; // the data is not synced yet.
                
                _selectedItemToAuction = auctionItems[_selectedItemToAuctionIndex];
            }

            if (!_selectedItemToAuction)
            {
                Debug.LogError("There is no item to auction!!");
                return;
            }
            
            _selectedSampleToAuctionComponent = _selectedItemToAuction.GetComponent<AuctionSample>();

            var itemAudioInfo = _selectedSampleToAuctionComponent.audioInfo;
            if(itemAudioInfo)
            {
                auctionAudioSource.PlayOneShot(itemAudioInfo);
                SendCustomEventDelayedSeconds("DisplaySpecificAuctionItem",
                    itemAudioInfo.length);
            }
            else
            {
                DisplaySpecificAuctionItem();
            }
        }

        public void DisplaySpecificAuctionItem()
        {
            //The item would automatically start rotating when it is enabled.
            
            if (!_selectedSampleToAuctionComponent)
            {
                Debug.LogError("There is no AuctionItem component on " + _selectedItemToAuction.name);
                return;
            }
            
            _selectedSampleToAuctionComponent.SwitchRenderers();
            /*if(debugMode)
                Debug.Log("Auction Manager:  {_selectedItemToAuctionComponent.isBought: " + selectedSampleToAuctionComponent.isBought + "}");
            
            selectedSampleToAuctionComponent.isBought = false;
            selectedSampleToAuctionComponent.SetKinematic(true);*/

//TODO              _selectedItemToAuction.transform.position = showItemPosition.transform.position;

            // tween = _selectedItemToAuction.GetComponent<NukoTweenEngine>();
            
            /*if (Networking.IsOwner(gameObject))
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayItemAudioInfoForAll));
            }*/
           
            _rotateTweenId = tweenManager.RotateTo(
                _selectedItemToAuction, new Vector3(0, 360, 0), 360f / auctionItemRotateYSpeed, 
                0f, tweenManager.EaseLinear, false);
                
            tweenManager.LoopIncremental(_rotateTweenId, -1);
                
            if(auctionInfoUI)
                auctionInfoUI.text = "Auction Starts: " + _selectedItemToAuction.name;

            _selectedItemToAuctionIndex = System.Array.IndexOf(auctionItems, _selectedItemToAuction); // Set to actual index
            _isDisplayingItem = true;
            _auctionDisplayStartTime = Time.time;
            
           // RequestSerialization();
           if (Networking.IsOwner(gameObject))
           {
               // Broadcast to all clients to synchronize item visibility
               SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(SyncItemVisibility));
               RequestSerialization();
           }
        }
        
        public void PlayItemAudioInfoForAll()
        {
            if (_selectedSampleToAuctionComponent && _selectedSampleToAuctionComponent.audioInfo)
            {
                auctionAudioSource.PlayOneShot(_selectedSampleToAuctionComponent.audioInfo);
            }
        }
        
       // [Button("Debug/RestClicks")]
        private void ResetAllUnitClickCounts()
        {
            // Traverse allUnits and reset the clickNum for each UnitClickCounter component
            foreach (var unit in auctionButtons)
            {
                if(debugMode && unit && unit.clickNum != 0)
                    Debug.Log("Resetting "+ unit.name + " from " + unit.clickNum + " to 0");
                
                if (unit)
                    unit.OnReset();
            }
        }
        

        private AuctionButton GetAuctionWinner()
        {
            var maxCCs = new AuctionButton[0]; //Array.Empty is not allowed!
            var maxClickNum = 0;
            foreach (var cc in auctionButtons)
            {
                if (!cc)
                {
                    Debug.LogWarning("lost reference, continue..");
                    continue;
                }
                if (debugMode)
                {
                    Debug.Log("Auction Manager: cc: " + cc.name + " clickNum: " + cc.clickNum);
                }
                if (cc.clickNum > maxClickNum)
                {
                    maxCCs = new[] { cc };
                    maxClickNum = cc.clickNum;
                }
                else if (maxClickNum > 0 && cc.clickNum == maxClickNum) 
                {
                    maxCCs = Utilities.Array.AddTo(maxCCs, cc);
                }
            }

            if(debugMode)
                Debug.Log("Auction Manager: maxClickNum: " + maxClickNum + " maxCCs: " + maxCCs.Length);
            
            switch (maxCCs.Length)
            {
                case 0:
                    //unsold in the auction operations:
                    //SwitchToNextItem();
                    if(debugMode)
                        Debug.Log("Auction Manager: No Winner");
                    return null;
                case 1:
                    //There is ONE actual winner:
                    if(debugMode)
                        Debug.Log("Auction Manager: sole winner: " + maxCCs[0].name);
                    return maxCCs[0];
                default: //We can not use case >1 yet.. QAQ 
                    if (maxClickNum == 0)
                    {
                        
                    }
                    if (maxCCs.Length > 1)
                    {
                        if (debugMode)
                        {
                            var debuglog = "Auction Manager: Multi winners with value " + maxClickNum + ": ";
                            foreach (var cc in maxCCs)
                            {
                                debuglog += cc.name + ", ";
                            }
                            Debug.Log(debuglog);
                        }
                        //When there are multiple winners, randomly choose one.
                        return maxCCs[UnityEngine.Random.Range(0, maxCCs.Length)];
                    }
                    else
                    {
                        Debug.LogError("Invalid number of maxCCs: " + maxCCs.Length);
                        return null;
                    }
            }
        }

        public void PlayAuctionStartAnnouncement()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayAnnouncementForAll));
        }
        
        public void PlayAnnouncementForAll()
        {
            if (auctionPrepareAnnouncementAudioClip)
            {
                auctionAudioSource.PlayOneShot(auctionPrepareAnnouncementAudioClip);
            }
        }
        
        private void SwitchToNextItem()
        {
            if(auctionInfoUI)
                auctionInfoUI.text = "No participants. Switching to the next auction item...";
            //alrUnsoldOnce = true;
            
            if (_selectedItemToAuction)
                _selectedItemToAuction.SetActive(false);
            
            //We decided not to immediately show the next auction item when one is unsold.
           // SendCustomEventDelayedSeconds(nameof(DisplayAuctionItem), switchToNextAuctionTime);

            /*if (_selectedItemToAuctionIndex < auctionItems.Length)
            {
                _selectedItemToAuction = auctionItems[_selectedItemToAuctionIndex];
                _selectedItemToAuction.SetActive(true);
                _selectedItemToAuction.GetComponent<AuctionItem>().isBought = false;
                _selectedItemToAuction.transform.position = showItemPosition.transform.position;

                auctionInfoUI.text = "Auction started: " + _selectedItemToAuction.name;
                _selectedItemToAuctionIndex++;
                _isDisplayingItem = true;
                _auctionDisplayStartTime = Time.time;
            }
            else
            {
                auctionInfoUI.text = "Auction ended.";
                _selectedItemToAuctionIndex = 0;
                _isDisplayingItem = false;
            }*/
        }

        [Button("Get All Buttons")]
        private void InitializeUnits()
        {
            auctionButtons = allUnitsContainer.GetComponentsInChildren<AuctionButton>();
            Debug.Log("Manager reached 2-2");
        }

        private void InitializeAuctionItems()
        {
            foreach (var auctionItem in auctionItems)
            {
                auctionItem.SetActive(false);
            }
        }

        public void ResetAuctionedItems()
        {
            auctionedItemsToday = new GameObject[0];
            if(debugMode)
                Debug.Log("Reset Randomized.");
        }

        public GameObject GetRandomAuctionItem()
        {
            
            if (debugMode)
            {
                var itemList = "";
                foreach (var item in auctionItems)
                {
                    itemList += item.name + ", ";
                }
                Debug.Log("Auction Item List: " + itemList + " Length is " + auctionItems.Length);
            }
            
            if (auctionItems.Length == 0)
            {
                Debug.LogError("There is no item in the list!");
            }
            
            
            if (auctionedItemsToday.Length >= auctionItems.Length)
                ResetAuctionedItems();
            
            var index = Random.Range(0, auctionItems.Length);
            var go = auctionItems[index];
            while (Array.GameObjectContains(auctionedItemsToday, go) || !go.GetComponent<AuctionSample>().CheckPoolAvailability())
            {
                index = Random.Range(0, auctionItems.Length);
                go = auctionItems[index];
                //Because the number of auctionItems would not be too much, so just use while.
            }

            Array.AddTo(auctionedItemsToday, go);
            _selectedItemToAuctionIndex = index;
            RequestSerialization();
            
            if(debugMode)
                Debug.Log("The object to show is " + go.name);
            
            return go;
            
        }

        public void SetCanAuction(bool target)
        {
            if (!Networking.IsOwner(gameObject)) return;
            
            if(debugMode)
                Debug.Log("Now Set Can Auction to: " + target);
            canDoAuction = target;
            RequestSerialization();
        }

        public void AddItemToList(GameObject item)
        {
            //AddTo(auctionItems, item);
            var newArray = new GameObject[auctionItems.Length + 1]; //Very wired, T as GameObject is not working here.
                
            for (int i = 0; i < auctionItems.Length; i++)
            {
                newArray[i] = auctionItems[i];
            }
                
            newArray[auctionItems.Length] = item;
            auctionItems = newArray;
            
            if(debugMode) Debug.Log($"After adding {item.name}, the list length is {auctionItems.Length}");
        }
        
        public void SyncItemVisibility()
        {
            if (_selectedSampleToAuctionComponent) {
                _selectedSampleToAuctionComponent.ForceUpdateRenderers(true);
            }
        }
        
        public override void OnDeserialization() //TODO: Index is updating but showing is not.
        {
            // Debug visibility for troubleshooting
            if(auctionDebugText)
                auctionDebugText.text = $"OnDeserialization: canDoAuction={canDoAuction}, _isDisplayingItem={_isDisplayingItem}, index={_selectedItemToAuctionIndex}";
    
            // Handle auction state on non-owner clients
            if (!Networking.IsOwner(gameObject))
            {
                // If we should start an auction
                if (canDoAuction && !_isDisplayingItem)
                {
                    DisplayAuctionItem();
                    canDoAuction = false;
                }
                // If we're already displaying an item, ensure it's visible
                else if (_isDisplayingItem && _selectedItemToAuctionIndex >= 0 && _selectedItemToAuctionIndex < auctionItems.Length)
                {
                    // If our local reference doesn't match the synced index
                    if (_selectedItemToAuction != auctionItems[_selectedItemToAuctionIndex])
                    {
                        _selectedItemToAuction = auctionItems[_selectedItemToAuctionIndex];
                        _selectedSampleToAuctionComponent = _selectedItemToAuction.GetComponent<AuctionSample>();
                
                        // Explicitly make item visible for this client
                        _selectedSampleToAuctionComponent.ForceUpdateRenderers(true);
                
                        // Restart rotation animation
                        _rotateTweenId = tweenManager.RotateTo(
                            _selectedItemToAuction, new Vector3(0, 360, 0), 360f / auctionItemRotateYSpeed, 
                            0f, tweenManager.EaseLinear, false);
                    
                        tweenManager.LoopIncremental(_rotateTweenId, -1);
                
                        if(auctionInfoUI)
                            auctionInfoUI.text = "Auction Starts: " + _selectedItemToAuction.name;
                    }
                }
            }
        }
        
        /*public override void OnDeserialization()
        {
            // If _selectedItemToAuctionIndex changed and we're not the owner
            if (!Networking.IsOwner(gameObject) && _selectedItemToAuctionIndex >= 0 && _selectedItemToAuctionIndex < auctionItems.Length)
            {
                if (canDoAuction && !_isDisplayingItem)
                {
                    DisplayAuctionItem();
                    canDoAuction = false; // 确保只调用一次
                }
                /#1#/ Check if we need to update our local reference
                if (_selectedItemToAuction != auctionItems[_selectedItemToAuctionIndex])
                {
                    _selectedItemToAuction = auctionItems[_selectedItemToAuctionIndex];
                    _selectedSampleToAuctionComponent = _selectedItemToAuction.GetComponent<AuctionSample>();
            
                    // Make sure the item is visible for this client
                    _selectedSampleToAuctionComponent.ForceUpdateRenderers(true);
                }#1#
            }
        }*/
    }
}