using System;
using Auction;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using ProjectUtilities;
using VRC.SDKBase;
using VRC.Udon;
using VRRefAssist;

namespace Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    [RequireComponent(typeof(Collider)),RequireComponent(typeof(Rigidbody))]
    public class Item : UdonSharpBehaviour
    {
        [InfoBox("The hint text in the HUD")]
        [ReadOnly] public string hudHintText = "Use";
        public ItemCategory itemCategory;
        
        [Required]
        public Sprite icon;

        [Title("Inventory","TODO, How to find the correct one?")] [SerializeField]
        private InventoryManager inventoryManager;
        
        [Title("Sample-Parent")]
        [ReadOnly,SerializeField] private AuctionItemManager auctionItemManager;
        [ReadOnly,SerializeField] private AuctionSample auctionSample;
        private Transform auctionParent;
        
        [Title("Item Setting")]
        [SerializeField,ReadOnly, InfoBox("Change this from the sample")] 
        private bool scaleThisItemAfterBought = true;
        [SerializeField, ShowIf("scaleThisItemAfterBought"),
         InfoBox("Change this from the sample"),ReadOnly] 
        private Vector3 sizeAfterBought = Vector3.one;
        
        // Only sync the flying state and timing - NO SCALE SYNCING
        [UdonSynced] private bool _isFlying = false;
        [UdonSynced] private float _flyStartTime = 0f;
        [UdonSynced] private float _flyDuration = 5f;
        
        // Local animation variables (completely local, never synced)
        private Vector3 _startScale = Vector3.one;
        private Vector3 _targetScale = Vector3.one;
        private bool _animationInitialized = false;
        
        [Title("Components")]
        private Rigidbody _rb;
        
        [Title("Tween")] 
        [ReadOnly] public TweenManager tween;

        private void OnValidate()
        {
            if (GetComponent<AuctionSample>() || (auctionSample && auctionParent)) return;
            auctionParent = transform.parent;
            auctionSample = auctionParent.GetComponent<AuctionSample>();
        }

        public void InitItem(AuctionSample sample, bool pChangeScale, Vector3 pAfterBoughtSize)
        {
            auctionParent = sample.transform;
            auctionSample = sample;
            enabled = true;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            scaleThisItemAfterBought = pChangeScale;
            if (pChangeScale) sizeAfterBought = pAfterBoughtSize;
        }
        
        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            
            if (!_rb)
                Debug.LogError("Rigidbody is missing on " + gameObject.name);
        } 

        private void Update()
        {
            // Handle local scale animation - this runs independently on each client
            if (_isFlying && scaleThisItemAfterBought)
            {
                // Initialize animation parameters when flying starts
                if (!_animationInitialized)
                {
                    _startScale = transform.localScale;
                    _targetScale = sizeAfterBought;
                    _animationInitialized = true;
                }
                
                // Calculate progress based on synced start time
                var timePassed = Time.time - _flyStartTime;
                
                if (timePassed >= 0f && timePassed <= _flyDuration)
                {
                    var progress = Mathf.Clamp01(timePassed / _flyDuration);
                    progress = EaseInOutSine(progress);
                    
                    // Apply scale animation locally - no networking!
                    var newScale = Vector3.Lerp(_startScale, _targetScale, progress);
                    transform.localScale = newScale;
                }
                else if (timePassed > _flyDuration)
                {
                    // Animation complete - set final scale
                    transform.localScale = _targetScale;
                }
            }
            else if (!_isFlying && _animationInitialized)
            {
                // Flying ended - clean up
                _animationInitialized = false;
                
                if (scaleThisItemAfterBought)
                {
                    transform.localScale = sizeAfterBought;
                }
            }
        }
        
        // Simple implementation of ease in-out sine for consistent animation
        private float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
        }

        public override void Interact()
        {
            if (!inventoryManager)
            {
                Debug.LogWarning("No Inventory Manager found on " + name);
            }
            Debug.Log($"{name} is interacting with {Networking.LocalPlayer.displayName}");
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        public void StartFlyingToPlayer(Transform targetTransform)
        {
            Debug.Log($"{name} Start Flying to the player.");
            
            // Take ownership for network sync
            if (!Networking.IsOwner(gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            
            // Set synced variables for timing synchronization
            _flyDuration = auctionItemManager.deliveryFlyDuration;
            _flyStartTime = Time.time;
            _isFlying = true;
            
            // Position tween (this gets synced via Object Sync automatically)
            var flyTargetPos = targetTransform.position + new Vector3(0, auctionItemManager.deliveryYOffset, 0);
            tween.MoveTo(gameObject, flyTargetPos, _flyDuration, 0f, tween.EaseInOutSine, false);
            
            // Only owner handles the delayed call for ending
            if (Networking.IsOwner(gameObject))
            {
                tween.DelayedCall(target: this, nameof(EndFlying), _flyDuration);
            }

            // Setup physics
            if (!_rb)
            {
                _rb = GetComponent<Rigidbody>();
                if(!_rb) Debug.LogError($"There is no rigidbody on {name}");
            }
            _rb.isKinematic = false;
            _rb.useGravity = true;
            Debug.Log($"Setting {name} to kinematic false.");
        }
        
        [Preserve]
        private void EndFlying()
        {
            Debug.Log($"Ending flying for {name}");
            
            if (Networking.IsOwner(gameObject))
            {
                _isFlying = false;
                // This will sync automatically due to Continuous mode
            }
            
            // Ensure final scale is correct locally
            if (scaleThisItemAfterBought)
            {
                transform.localScale = sizeAfterBought;
            }
            
            SetKinematic(false);
            _rb.useGravity = true;
        }
        
        public void SetKinematic(bool target)
        {
            _rb.isKinematic = target;
        }

        public void OnConsume()
        {
            auctionSample.OnConsume(gameObject);
            transform.parent = auctionParent;
        }

        public override void OnDeserialization()
        {
            base.OnDeserialization();
            
            // When flying state changes from network, just trigger local animation setup
            // Do NOT apply any scale values from network - let each client handle it locally
            if (_isFlying && !_animationInitialized && scaleThisItemAfterBought)
            {
                // This will be picked up in Update() to start the local animation
                _animationInitialized = false; // Will be set to true in Update()
            }
        }
    }
}