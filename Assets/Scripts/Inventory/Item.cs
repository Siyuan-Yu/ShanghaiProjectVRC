
using System;
using Auction;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Utilities;
using VRC.SDKBase;
using VRC.Udon;
using VRRefAssist;

namespace Inventory
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    [RequireComponent(typeof(Collider)),RequireComponent(typeof(Rigidbody))]
    public class Item : UdonSharpBehaviour
    {
        [InfoBox("The hint text in the HUD")]//"Set the hint text from auction sample")]
        [ReadOnly] public string hudHintText = "Use";
        public ItemCategory itemCategory;
        
        [Required]
        public Image icon;

        [Title("Inventory","TODO, How to find the correct one?")] [SerializeField]//, Required]
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
        [UdonSynced] private Vector3 currentScale;
        
        [Title("Components")]
        private Rigidbody _rb;
        
        [Title("Tween")] 
        [ReadOnly] public TweenManager tween;
        private bool _isFlying = false;

        private void OnValidate()
        {
            if (GetComponent<AuctionSample>() || (auctionSample && auctionParent)) return;
            auctionParent = transform.parent;
            auctionSample = auctionParent.GetComponent<AuctionSample>();
        }

        public void InitItem(AuctionSample sample, bool pChangeScale, Vector3 pAfterBoughtSize) //p for parameters
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
            
            /*if(!auctionSample || !auctionParent)
            {
                auctionParent = transform.parent;
                auctionSample = auctionParent.GetComponent<AuctionSample>();
            }*/
            
            //if(!auctionSample || !auctionParent) Debug.LogError($"{name} does not have sample connected!"); 
            
        } 

        private void Update()
        {
            if (_isFlying)
            {
                currentScale = transform.localScale;
            }
        }

        public override void Interact()
        {
            if (!inventoryManager)
            {
                Debug.LogWarning("No Inventory Manager found on " + name);
            }
            Debug.Log($"{name} is interacting with {Networking.LocalPlayer.displayName}");
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            //inventoryManager.AddToInventory(this, Networking.LocalPlayer);
            //TODO: FInd the local player and add it to the player's inventory
        }
        
        public void StartFlyingToPlayer(Transform targetTransform)
        {
            _isFlying = true;
            Debug.Log($"{name} Start Flying to the player.");
            //TODO: Debug
            var flyTargetPos = targetTransform.position + new Vector3(0,auctionItemManager.deliveryYOffset,0);
            tween.MoveTo(gameObject, flyTargetPos, auctionItemManager.deliveryFlyDuration, 0f, tween.EaseInOutSine, false);
            if(scaleThisItemAfterBought && Networking.IsOwner(gameObject))
            {
                tween.LocalScaleTo(gameObject, sizeAfterBought, auctionItemManager.deliveryFlyDuration, 0f,
                    tween.EaseInOutSine, false);
                tween.DelayedCall(target:this, nameof(EndFlying),auctionItemManager.deliveryFlyDuration);
            }
            // SendCustomEventDelayedSeconds("EndFlying",auctionItemManager.deliveryFlyDuration); //doesn't work..
            
            

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
            Debug.Log($"Setting {name} to kinematic false.");
            _isFlying = false;
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
            if(_isFlying)
                transform.localScale = currentScale;
        }
    }
}
