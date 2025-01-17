﻿
using System;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using NukoTween;

namespace Auction
{
    public class AuctionItem : UdonSharpBehaviour
    {
        [InfoBox("If it is empty, we will use the name of the object","@displayName==''")]
        public string displayName;
        [Title("Manager")]
        [HideInInspector] public AuctionItemManager actionItemManager;
         // Auction State
        [Title("Auction State")]
        [UdonSynced] public bool isBought = false;
        [UdonSynced] public bool boughtPlaySound = false;
        [UdonSynced] public int ownerPlayerID;

        [Space, ReadOnly] public bool isAuctionedToday;

        [Title("Item Setting")]
        [SerializeField,InfoBox("If this item should be re-scaled after bought, check this:")] 
        private bool scaleThisItemAfterBought = true;
        [SerializeField,ReadOnly, ShowIf("scaleThisItemAfterBought"),
         InfoBox(
             "The scale after this item get auctioned. If it should be bigger, then set bigger, vice verse. If it should stay the size, copy the scale from transform")] 
        private Vector3 sizeAfterBought;

        private Rigidbody _rb;
       // public bool isBigItem = false; // 大物品标识，默认为 false 表示小物品

        public int unitIndexToGo;
        
        [Title("Points of this Item")]
        [ReadOnly] public PointSystem pointSystem;
        [UdonSynced] public int itemPointValue;
        [UdonSynced] private bool _canAddPoint = true;
    
        [Title("Separate Sounds of this item")]
        public AudioSource audioSource;
        public AudioClip audioToPlay;
    
        public int localPoint; //local player's point
        [Title("Fly to player Tween Settings")]
        private NukoTweenEngine tween;
        private int _flyToPlayTweenId;
        [SerializeField,Unit(Units.Second)] private float flyDuration = 3;
        private bool isFlying;
        private Vector3 flyTargetPos;
        [SerializeField,Range(0,10f)] private float arriveDistanceThreshold;
        
       // public Text collideIDText;

       // public bool isPheonix; //TODO move to a inherit
       //public bool setKinetic = false;

        public GameObject goToUnit;

        void Start()
        {
            if (displayName == "") displayName = name;
            ownerPlayerID = 0; //set default value until it is bought.
            _rb = GetComponent<Rigidbody>();
            
            if (_rb) return;
            Debug.LogError("Rigidbody is missing on " + gameObject.name);
            //_rb = gameObject.AddComponent<Rigidbody>(); //Nope, not allowed
        }

        private void FixedUpdate()
        {
            /*if (!canAddPoint && !isPheonix)
            {
                addPointVal = 0;
                if (!isBigItem) // 如果不是大物品，才执行缩小逻辑
                {
                    transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // 小物品缩小
                }
                // 对于大物品，保持原始大小，不缩小
            }*/
            CheckFlying();
        }

        

        public void StartFlyingToPlayer(Transform targetTransform)
        {
            isBought = true;
            flyTargetPos = targetTransform.position;
            _flyToPlayTweenId = tween.MoveTo(gameObject, flyTargetPos, flyDuration, 0f, tween.EaseLinear, false);
        }
        public GameObject defaultDeliverPlace;

        private void CheckFlying()
        {
            if (!isFlying) return;

            if ((transform.position - flyTargetPos).magnitude > arriveDistanceThreshold) return;
            
            tween.Kill(_flyToPlayTweenId);
            //Arrive the target position now
            SetKinematic(false);
            isFlying = false;
            _rb.useGravity = true;
            
            if (!boughtPlaySound)
            {
                boughtPlaySound = true;
                audioSource.clip = audioToPlay;
                audioSource.Play();
            }
        }
        public void SetKinematic(bool target)
        {
            _rb.isKinematic = target;
        }
        public override void OnPlayerTriggerEnter(VRCPlayerApi other)
        {
            //TODO
            /*VRCPlayerApi player = other;
            // Check if the player exists and is active
            if (player != null && !isPheonix)
            {
                _rb.isKinematic = false;
                ownerPlayerID = VRCPlayerApi.GetPlayerId(player);

                if (canAddPoint)
                {
                    if (addPointVal != 0 && addPointVal != null)
                    {
                        collideIDText.text = localPoint.ToString();
                        localPoint = (int)pointSystem.GetProgramVariable("points");

                        localPoint += addPointVal;
                        pointSystem.SetProgramVariable("points", localPoint);
                        
                    }
                    canAddPoint = false;
                }
            }*/
        }
    }
}
