
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
        
        [Title("Components")]
        private Rigidbody _rb;
        
        [Title("Tween")] 
        [ReadOnly] public TweenManager tween;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            
            if (!_rb)
                Debug.LogError("Rigidbody is missing on " + gameObject.name);
            
            
            if(!auctionSample || !auctionParent) Debug.LogError($"{name} does not have sample connected!"); 
            
        }
        
        public override void Interact()
        {
            if (!inventoryManager)
            {
                Debug.LogWarning("No Inventory Manager found on " + name);
            }
            Debug.Log($"{name} is interacting with {Networking.LocalPlayer.displayName}");
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            inventoryManager.AddToInventory(this, Networking.LocalPlayer);
            //TODO: FInd the local player and add it to the player's inventory
        }
        
        public void StartFlyingToPlayer(Transform targetTransform)
        {
            Debug.Log($"{name} Start Flying to the player.");
            //TODO: Debug
            var flyTargetPos = targetTransform.position + new Vector3(0,auctionItemManager.deliveryYOffset,0);
            tween.MoveTo(gameObject, flyTargetPos, auctionItemManager.deliveryFlyDuration, 0f, tween.EaseInOutSine, false);
            if(scaleThisItemAfterBought)
                tween.LocalScaleTo(gameObject, sizeAfterBought, auctionItemManager.deliveryFlyDuration, 0f, tween.EaseInOutSine, false);
            
            // SendCustomEventDelayedSeconds("EndFlying",auctionItemManager.deliveryFlyDuration); //doesn't work..
            tween.DelayedCall(target:this, nameof(EndFlying),auctionItemManager.deliveryFlyDuration);

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
            SetKinematic(false);
            _rb.useGravity = true;
        }
        
        public void SetKinematic(bool target)
        {
            _rb.isKinematic = target;
        }
    }
}

