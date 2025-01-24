
using System;
using NukoTween;
using Sirenix.OdinInspector;
using TMPro;
using UdonSharp;
using VRRefAssist;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using Array = Utilities.Array;
using Random = UnityEngine.Random;

namespace Auction
{
    //[Singleton]
    public class AuctionItemManager : UdonSharpBehaviour
    {
        [Title("Auction Settings"),ReadOnly] public GameObject[] auctionItems;
        private GameObject[] auctionedItemsToday;
        private GameObject _selectedItemToAuction;
        private int _selectedItemToAuctionIndex  = 0;
        private AuctionItem _selectedItemToAuctionComponent;
        private UnitClickCounter _winnerThisRound;
        
       // [InfoBox()]
        [SerializeField] private float auctionInfoDisplayDuration = 15f;
        [InfoBox("When displaying an item, the object would rotate until it is bought or expired"),Unit(Units.DegreesPerSecond)]
        public float auctionItemRotateYSpeed;
        
        [ReadOnly] public bool canDoAuction; // controlled by DNE System
        private bool _isDisplayingItem;
        private float _auctionDisplayStartTime;

       // [InfoBox("When one is unsold, the time to wait to switch to next auction item"),Unit(Units.Second)]
       // public float switchToNextAuctionTime = 3f;

        //private bool alrUnsoldOnce;
        
        [Title("Unit Management")]
        [SerializeField] private GameObject allUnitsContainer;
        [SerializeField,ReadOnly] private UnitClickCounter[] allUnitsClickCounters;
        /*private GameObject[] validUnits;
        [SerializeField, ReadOnly] private int validUnitCount = 0;*/

        [Title("UI Elements")]
        [SerializeField] private TextMeshProUGUI auctionWinnerInfoUI;
        [SerializeField] private TextMeshProUGUI collideIDTextMeshProUGUI;
        [SerializeField] private TextMeshProUGUI idTextMeshProUGUI;
        [SerializeField] private TextMeshProUGUI auctionInfoUI;
        
        [Title("Audio Settings")]
        [SerializeField,Required] private AudioSource auctionAudioSource;
        [SerializeField] private AudioClip auctionCountdown;
        [SerializeField] private float countdownAudioDuration = 3f;
        private bool _playedCountdownThisTime = false;

        [TitleGroup("Delivery Settings")]
        [SerializeField] private GameObject defaultDeliverPlace;
        [SerializeField] private float smallItemScaleMultiplier = 0.05f;
        
        [TitleGroup("Delivery Settings/Tweens")] 
        private NukoTweenEngine tween;

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

        private void Awake()
        {
            auctionItems = new GameObject[0];
        }

        private void Start() //In the Script Execution Order, Item is earlier than Manager.
        {
            if(! auctionAudioSource) Debug.LogError("Missing Audio Source on " + gameObject.name);
            InitializeUnits();
            //InitializeAuctionItems();
            //SendCustomEventDelayedSeconds(nameof(InitializeAuctionItems), 1f);
        }

        private void Update()
        {
            if(debugMode)
                Debug.Log("Can Auction: " + canDoAuction + " " + "is Displaying Item: " + _isDisplayingItem);
            if (canDoAuction && !_isDisplayingItem)
            {
                DisplayAuctionItem();
                canDoAuction = false; // 确保只调用一次
            }

            if (_isDisplayingItem)
            {
                var timePassed = Time.time - _auctionDisplayStartTime;
                if (timePassed >= auctionInfoDisplayDuration - countdownAudioDuration && !_playedCountdownThisTime)
                {
                    auctionAudioSource.PlayOneShot(auctionCountdown);
                    _playedCountdownThisTime = true;
                }
                else if(timePassed >= auctionInfoDisplayDuration)
                {
                    _isDisplayingItem = false;
                    _winnerThisRound = GetAuctionWinner();
                   // tween.Kill(_rotateTweenId);
                    if (_winnerThisRound)
                    {
                        var generatedItem = Instantiate(_selectedItemToAuction, _selectedItemToAuction.transform.position, _selectedItemToAuction.transform.rotation);
                        _selectedItemToAuction.SetActive(false);
                        generatedItem.name = _selectedItemToAuction.name + " of " + _winnerThisRound.playerName;
                        _selectedItemToAuctionComponent.isAuctionedToday = true;
                        var generatedItemComponent = generatedItem.GetComponent<AuctionItem>();
                        generatedItemComponent.StartFlyingToPlayer(_winnerThisRound.transform);
                        
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
                        Debug.Log("There is no winner in this round.");
                    }
                }
                else
                {
                    if (auctionWinnerInfoUI)
                        auctionWinnerInfoUI.text = "Auction is going on for " + (auctionInfoDisplayDuration - timePassed).ToString("0.0") + " seconds";
                }
            }
            
        }

        public void DisplayAuctionItem()
        {
            Debug.Log("Auction: Try Start Displaying.");
            _winnerThisRound = null;

            ResetAllUnitClickCounts();
            
            _selectedItemToAuction = GetRandomAuctionItem();

            if (!_selectedItemToAuction)
            {
                Debug.LogError("There is no item to auction!!");
                return;
            }
            
            if(debugMode)
                Debug.Log("Auction Manager: Step 1");
            
            _selectedItemToAuction.SetActive(true);
            //The item would automatically start rotating when it is enabled.
            _selectedItemToAuctionComponent = _selectedItemToAuction.GetComponent<Auction.AuctionItem>();
            if (!_selectedItemToAuctionComponent)
            {
                Debug.LogError("There is no AuctionItem component on " + _selectedItemToAuction.name);
            }
            
            if(debugMode)
                Debug.Log("Auction Manager: Step 2 {_selectedItemToAuctionComponent.isBought: " + _selectedItemToAuctionComponent.isBought + "}");
            
            _selectedItemToAuctionComponent.isBought = false;
            _selectedItemToAuctionComponent.SetKinematic(true);
            if(debugMode)
                Debug.Log("Auction Manager: Step 3");
//TODO              _selectedItemToAuction.transform.position = showItemPosition.transform.position;
            //_rotateTweenId = tween.RotateTo(_selectedItemToAuction, new Vector3(0, 360, 0), 360 / auctionItemRotateYSpeed,
           // 0f, tween.EaseInExpo, false); //TODO: Debug needed
            if(debugMode)
                Debug.Log("Auction Manager: Step 4");
           // tween.LoopIncremental(_rotateTweenId, -1);
            
            if(debugMode)
                Debug.Log("Auction Manager: Step 5");
            
            if(auctionInfoUI)
                auctionInfoUI.text = "Auction Starts: " + _selectedItemToAuction.name;

            _selectedItemToAuctionIndex++;
            _isDisplayingItem = true;
            _auctionDisplayStartTime = Time.time;
        

            _selectedItemToAuctionIndex = 0;
            
        }
        
        
        private void ResetAllUnitClickCounts()
        {
            // Traverse allUnits and reset the clickNum for each UnitClickCounter component
            foreach (var unit in allUnitsClickCounters)
            {
                var clickCounter = unit.GetComponent<UnitClickCounter>();
                if(debugMode)
                    Debug.Log("Resetting "+ unit.name + " from " + clickCounter.clickNum + " to 0");
                if (clickCounter)
                {
                    clickCounter.clickNum = 0;
                }
            }
        }
        

        private UnitClickCounter GetAuctionWinner()
        {
            var maxCCs = new UnitClickCounter[0]; //Array.Empty is not allowed!
            var maxClickNum = 0;
            foreach (var cc in allUnitsClickCounters)
            {
                if (cc.clickNum > maxClickNum)
                {
                    maxCCs = new[] { cc };
                    maxClickNum = cc.clickNum;
                }
                else if(cc.clickNum == maxClickNum)
                {
                    maxCCs = Utilities.Array.AddTo(maxCCs, cc);
                }
            }

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
                    if (maxCCs.Length > 1)
                    {
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

        // Centralized logic or utility methods
        private void InitializeUnits()
        {
            allUnitsClickCounters = allUnitsContainer.GetComponentsInChildren<UnitClickCounter>();
        }

        private void InitializeAuctionItems()
        {
            foreach (var auctionItem in auctionItems)
            {
                auctionItem.SetActive(false);
            }
        }

        public void ResetAuctionItems()
        {
            auctionedItemsToday = new GameObject[0];
            
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
                Debug.Log("Auction Item List: " + itemList);
            }
            
            if (auctionItems.Length == 0)
            {
                Debug.LogError("There is no item in the list!");
            }
            if (auctionedItemsToday.Length >= auctionItems.Length)
                ResetAuctionItems();
            
            var go = auctionItems[Random.Range(0, auctionItems.Length)];
            while (Array.GameObjectContains(auctionedItemsToday, go))
            {
                go = auctionItems[Random.Range(0, auctionItems.Length)];
                //Because the number of auctionItems would not be too much, so just use while.
            }

            Array.AddTo(auctionedItemsToday, go);
            
            if(debugMode)
                Debug.Log("The object to show is " + go.name);
            
            return go;
            
        }

        public void SetCanAuction(bool target)
        {
            if(debugMode)
                Debug.Log("Now Set Can Auction to: " + target);
            canDoAuction = target;
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
            //Debug.Log($"After adding {item.name}, the list length is {auctionItems.Length}");
        }
        
    }
}
