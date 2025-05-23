using System;
using System.Globalization;
using System.Net;
using Inventory;
using NukoTween;
using ProjectUtilities;
using Sirenix.OdinInspector;
using TMPro;
using UdonSharp;
using VRRefAssist;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using Array = ProjectUtilities.Array;
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
        [UdonSynced] private int _selectedItemToAuctionIndex = -1;
        private AuctionSample _selectedSampleToAuctionComponent;
        private AuctionButton _winnerThisRound;
        
        // Add synced boolean for item visibility
        [UdonSynced] private bool _itemVisible = false;
        
        [SerializeField, Unit(Units.Second), InfoBox("$_auctionDurationLengthConvert"),OnValueChanged("_auctionDurationLengthConverter")] 
        private float auctionDuration = 15f;
        [InfoBox("When displaying an item, the object would rotate until it is bought or expired"),Unit(Units.DegreesPerSecond)]
        public float auctionItemRotateYSpeed;
        
        [ReadOnly,UdonSynced] public bool canDoAuction;
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

        [Title("Unit Management")]
        [SerializeField] private GameObject allUnitsContainer;
        [SerializeField,ReadOnly,Searchable] private AuctionButton[] auctionButtons;

        [Title("UI Elements")]
        [SerializeField] private TextMeshProUGUI auctionWinnerInfoUI;
        [SerializeField] private TextMeshProUGUI auctionInfoUI;
        
        [TitleGroup("Audio Settings")]
        [SerializeField,Required] private AudioSource auctionAudioSource;

        [TitleGroup("Audio Settings/Prepare Notification")] [InfoBox("When the time reaches the moment set in the DNE Sys, the clip would be played.")]
        [SerializeField,Required] private AudioClip auctionPrepareAnnouncementAudioClip;
        
        [TitleGroup("Audio Settings/Countdown")]
        [SerializeField] private AudioClip auctionCountdown;
        [TitleGroup("Audio Settings/Countdown")] [SerializeField] private float countdownAudioDuration = 3f;
        private bool _playedCountdownThisTime = false;

        [TitleGroup("Delivery Settings"),Unit(Units.Meter)]
        public float deliveryYOffset = 1;
        [TitleGroup("Delivery Settings"),Unit(Units.Second)]
        public float deliveryFlyDuration = 5f;
        
        [TitleGroup("Delivery Settings/Tweens"),ReadOnly] 
        public TweenManager tweenManager;

        private int _rotateTweenId;
        private int _flyToPlayerTweenId;

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

        private void Start()
        {
            auctionedItemsToday = new GameObject[0];
            
            if(! auctionAudioSource) Debug.LogError("Missing Audio Source on " + gameObject.name);
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
            if(!Networking.IsOwner(gameObject)) return;
            
            if (canDoAuction && !_isDisplayingItem)
            {
                DisplayAuctionItem();
                canDoAuction = false;
            }
            
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
                    EndAuction();
                }
                else
                {
                    if (auctionWinnerInfoUI)
                        auctionWinnerInfoUI.text = "Auction is going on for " + (auctionDuration - timePassed).ToString("0.0") + " seconds";
                }
            }
        }

        private void EndAuction()
        {
            _isDisplayingItem = false;
            _winnerThisRound = GetAuctionWinner();
            
            if (_winnerThisRound)
            {
                _selectedSampleToAuctionComponent.StartFlyingToPlayer(_winnerThisRound.transform);
                Debug.Log("There is winner and it is " + _winnerThisRound.name + " " + _winnerThisRound.playerName);
                
                if(_winnerThisRound.playerName == "")
                {
                    if (auctionWinnerInfoUI)
                        auctionWinnerInfoUI.text = "Winner: " + _winnerThisRound.name;
                }
                else
                {
                    if(auctionWinnerInfoUI)
                        auctionWinnerInfoUI.text = "Winner: " + _winnerThisRound.playerName;
                }
            }
            else
            {
                Debug.Log("There is no winner in this round.");
                if (auctionWinnerInfoUI)
                    auctionWinnerInfoUI.text = "No winner this round";
            }

            // Hide item for everyone
            _itemVisible = false;
            _selectedSampleToAuctionComponent.ForceUpdateRenderers(false);
            
            // Reset selection
            _selectedItemToAuction = null;
            _selectedSampleToAuctionComponent = null;
            _selectedItemToAuctionIndex = -1;
            
            RequestSerialization();
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
                if(_selectedItemToAuctionIndex == -1) return;
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
                SendCustomEventDelayedSeconds("DisplaySpecificAuctionItem", itemAudioInfo.length);
            }
            else
            {
                DisplaySpecificAuctionItem();
            }
        }

        public void DisplaySpecificAuctionItem()
        {
            if (!_selectedSampleToAuctionComponent)
            {
                Debug.LogError("There is no AuctionItem component on " + _selectedItemToAuction.name);
                return;
            }
            
            // Show item
            _itemVisible = true;
            _selectedSampleToAuctionComponent.ForceUpdateRenderers(true);
            
            // Start rotation animation
            _rotateTweenId = tweenManager.RotateTo(
                _selectedItemToAuction, new Vector3(0, 360, 0), 360f / auctionItemRotateYSpeed, 
                0f, tweenManager.EaseLinear, false);
                
            tweenManager.LoopIncremental(_rotateTweenId, -1);
                
            if(auctionInfoUI)
                auctionInfoUI.text = "Auction Starts: " + _selectedItemToAuction.name;

            _isDisplayingItem = true;
            _auctionDisplayStartTime = Time.time;
            _playedCountdownThisTime = false;
            
            if (Networking.IsOwner(gameObject))
            {
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
        
        private void ResetAllUnitClickCounts()
        {
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
            var maxCCs = new AuctionButton[0];
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
                    maxCCs = Array.AddTo(maxCCs, cc);
                }
            }

            if(debugMode)
                Debug.Log("Auction Manager: maxClickNum: " + maxClickNum + " maxCCs: " + maxCCs.Length);
            
            switch (maxCCs.Length)
            {
                case 0:
                    if(debugMode)
                        Debug.Log("Auction Manager: No Winner");
                    return null;
                case 1:
                    if(debugMode)
                        Debug.Log("Auction Manager: sole winner: " + maxCCs[0].name);
                    return maxCCs[0];
                default:
                    if (maxClickNum == 0)
                    {
                        return null;
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
            }

            Array.AddTo(auctionedItemsToday, go);
            _selectedItemToAuctionIndex = index;
            
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
            var newArray = new GameObject[auctionItems.Length + 1];
                
            for (int i = 0; i < auctionItems.Length; i++)
            {
                newArray[i] = auctionItems[i];
            }
                
            newArray[auctionItems.Length] = item;
            auctionItems = newArray;
            
            if(debugMode) Debug.Log($"After adding {item.name}, the list length is {auctionItems.Length}");
        }
        
        public override void OnDeserialization()
        {
            if(auctionDebugText)
                auctionDebugText.text = $"OnDeserialization: canDoAuction={canDoAuction}, _isDisplayingItem={_isDisplayingItem}, index={_selectedItemToAuctionIndex}, visible={_itemVisible}";
    
            // Handle auction state on non-owner clients
            if (!Networking.IsOwner(gameObject))
            {
                // Update item reference if index changed
                if (_selectedItemToAuctionIndex >= 0 && _selectedItemToAuctionIndex < auctionItems.Length)
                {
                    _selectedItemToAuction = auctionItems[_selectedItemToAuctionIndex];
                    _selectedSampleToAuctionComponent = _selectedItemToAuction.GetComponent<AuctionSample>();
                }
                
                // If we should start an auction
                if (canDoAuction && !_isDisplayingItem)
                {
                    DisplayAuctionItem();
                    canDoAuction = false;
                }
                
                // Update item visibility based on synced state
                if (_selectedSampleToAuctionComponent)
                {
                    _selectedSampleToAuctionComponent.ForceUpdateRenderers(_itemVisible);
                    
                    // Handle rotation animation for visible items
                    if (_itemVisible && _isDisplayingItem)
                    {
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
    }
}