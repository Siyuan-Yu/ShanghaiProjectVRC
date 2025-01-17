using System;
using System.Collections;
using NukoTween;
using Sirenix.OdinInspector;
using TMPro;
using UdonSharp;
using VRRefAssist;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace Auction
{
    [Singleton]
    public class AuctionItemManager : UdonSharpBehaviour
    {
        [Title("Auction Settings")] public GameObject[] auctionItems;
        private GameObject _selectedItemToAuction;
        private int _selectedItemToAuctionIndex  = 0;
        private AuctionItem _selectedItemToAuctionComponent;
        private UnitClickCounter _winnerThisRound;
        
        [SerializeField] private float auctionInfoDisplayDuration = 15f;
        [InfoBox("When displaying an item, the object would rotate until it is bought or expired"),Unit(Units.DegreesPerSecond)]
        public float auctionItemRotateYSpeed;
        
        [ReadOnly] public bool canDoAuction; // controlled by DNE System
        private bool _isDisplayingItem;
        private float _auctionDisplayStartTime;

        [InfoBox("When one is unsold, the time to wait to switch to next auction item"),Unit(Units.Second)]
        public float switchToNextAuctionTime = 3f;

        private bool alrUnsoldOnce;
        
        [Title("Unit Management")]
        [SerializeField] private GameObject allUnitsContainer;
        private UnitClickCounter[] allUnitsClickCounters;
        private GameObject[] validUnits;
        [SerializeField, ReadOnly] private int validUnitCount = 0;

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

        private void Start()
        {
            if(! auctionAudioSource) Debug.LogError("Missing Audio Source on " + gameObject.name);
            InitializeUnits();
            InitializeAuctionItems();

        }

        private void Update()
        {
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
                    _winnerThisRound = GetAuctionWinner();
                    tween.Kill(_rotateTweenId);
                    if (_winnerThisRound)
                    {
                        var generatedItem = Instantiate(_selectedItemToAuction, _selectedItemToAuction.transform.position, _selectedItemToAuction.transform.rotation);
                        _selectedItemToAuctionComponent.isAuctionedToday = true;
                        var generatedItemComponent = generatedItem.GetComponent<AuctionItem>();
                        generatedItemComponent.StartFlyingToPlayer(_winnerThisRound.transform);
                        
                        if(_winnerThisRound.playerName == "")
                            auctionWinnerInfoUI.text = "Winner: " + _winnerThisRound.name;
                        else
                            auctionWinnerInfoUI.text = "Winner: " + _winnerThisRound.playerName;
                    }
                }
            }
            
        }

        public void DisplayAuctionItem()
        {
            _winnerThisRound = null;
            if (_selectedItemToAuctionIndex < auctionItems.Length)
            {
                ResetAllUnitClickCounts();
                _selectedItemToAuction = auctionItems[_selectedItemToAuctionIndex];
                _selectedItemToAuction.SetActive(true);
                //The item would automatically start rotating when it is enabled.
                _selectedItemToAuctionComponent = _selectedItemToAuction.GetComponent<AuctionItem>();
                _selectedItemToAuctionComponent.isBought = false;
                _selectedItemToAuctionComponent.SetKinematic(true);
  //TODO              _selectedItemToAuction.transform.position = showItemPosition.transform.position;
                _rotateTweenId = tween.LocalRotateTo(_selectedItemToAuction, new Vector3(0, 360, 0), 360 / auctionItemRotateYSpeed,
                0f, tween.EaseInExpo, false); //TODO: Debug needed
                tween.LoopIncremental(_rotateTweenId, -1);
                auctionInfoUI.text = "Auction Starts: " + _selectedItemToAuction.name;

                _selectedItemToAuctionIndex++;
                _isDisplayingItem = true;
                _auctionDisplayStartTime = Time.time;
            }
            else
            {
                _selectedItemToAuctionIndex = 0;
            }
        }
        
        
        private void ResetAllUnitClickCounts()
        {
            // Traverse allUnits and reset the clickNum for each UnitClickCounter component
            foreach (var unit in allUnitsClickCounters)
            {
                var clickCounter = unit.GetComponent<UnitClickCounter>();
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
                    SwitchToNextItem();
                    return null;
                case 1:
                    //There is ONE actual winner:
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
            auctionInfoUI.text = "No participants. Switching to the next auction item...";
            alrUnsoldOnce = true;
            
            if (_selectedItemToAuction)
                _selectedItemToAuction.SetActive(false);
            
            
            SendCustomEventDelayedSeconds(nameof(DisplayAuctionItem), switchToNextAuctionTime);

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
            var childTransforms = allUnitsContainer.GetComponentsInChildren<Transform>();
            allUnitsClickCounters = new UnitClickCounter[childTransforms.Length - 1];
            
            for (var i = 1; i < childTransforms.Length; i++){
               //exclude the first, which is the container
               var counter = childTransforms[i].GetComponent<UnitClickCounter>();
               if(!counter) Debug.LogError("Missing UnitClickCounter on " + childTransforms[i].name);
               allUnitsClickCounters[i - 1] = counter;
            } 
        }

        private void InitializeAuctionItems()
        {
            foreach (var auctionItem in auctionItems)
            {
                auctionItem.SetActive(false);
            }
        }
    }
}
