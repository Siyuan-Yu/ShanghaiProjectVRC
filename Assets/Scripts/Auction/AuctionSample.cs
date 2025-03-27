
using System;
using Inventory;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using NukoTween;
using UnityEngine.Scripting;
using Utilities;
using VRC.Core.Pool;
using VRC.SDK3.Components;
using Array = Utilities.Array;

namespace Auction
{
    public class AuctionSample : UdonSharpBehaviour
    {
        [InfoBox("If it is empty, we will use the name of the object")]
        public string displayName;

        [Title("Manager")]
        // [HideInInspector] 
        [SerializeField]
        private AuctionItemManager auctionItemManager;
        
        [Title("Object Pool")]
        public VRCObjectPool objectPool;

        //public GameObject[] objectPoolArray;


        [Title("Tween")] [ReadOnly] public TweenManager tween;

        // Auction State
        [Title("Auction State")] [UdonSynced] public bool isBought = false;
        [UdonSynced] public bool boughtPlaySound = false;
        public AudioClip boughtPlayAudioClip;
        [UdonSynced] public int ownerPlayerID;

        [Space, ReadOnly] public bool isAuctionedToday;

        [Title("Item Setting")]
        [SerializeField,InfoBox("If this item should be re-scaled after bought, check this:")] 
        private bool scaleThisItemAfterBought = true;
        [SerializeField, ShowIf("scaleThisItemAfterBought"),
         InfoBox(
             "The scale after this item get auctioned. If it should be bigger, then set bigger, vice verse. If it should stay the size, copy the scale from transform")] 
        private Vector3 sizeAfterBought = Vector3.one;

        private Rigidbody _rb;
       // public bool isBigItem = false; // 大物品标识，默认为 false 表示小物品

        //public int unitIndexToGo;
        
        [Title("Points of this Item")]
        [ReadOnly] public PointSystem pointSystem;
        [UdonSynced] public int itemPointValue;
        [UdonSynced] private bool _canAddPoint = true;
    
        [Title("Separate Sounds of this item")]
        public AudioSource audioSource;
        public AudioClip audioInfo;
    
        public int localPoint; //local player's point
        [Title("Fly to player Tween Settings")]
       // private NukoTweenEngine tween;
        private int _flyToPlayTweenId;

        private int _scaleDuringFlyingTweenId;
        
        //[SerializeField,Unit(Units.Second)] private float flyDuration = 3;
        private bool isFlying;
        private Vector3 flyTargetPos;
        [SerializeField,Range(0,10f)] private float arriveDistanceThreshold;


        [Title("Misc")] public bool isPrototype = true;

        
       // public Text collideIDText;

       // public bool isPheonix; //TODO move to a inherit
       //public bool setKinetic = false;

        public GameObject goToUnit;

        private void Start()
        {
            if (displayName == "") displayName = name;
            ownerPlayerID = 0; //set default value until it is bought.
            _rb = GetComponent<Rigidbody>();
            
            if (!_rb)
                Debug.LogError("Rigidbody is missing on " + gameObject.name);


            if (!isPrototype) return;
            
            if(!auctionItemManager) Debug.LogError($"{name} does not have manager!"); 
            
            auctionItemManager.AddItemToList(gameObject); 
            gameObject.SetActive(false);
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
           // CheckFlying();
        }



        public void StartFlyingToPlayer(Transform targetTransform)
        {
            Debug.Log($"{name} Start Flying to the player.");
            //TODO: Debug
            isBought = true;
            flyTargetPos = targetTransform.position + new Vector3(0,auctionItemManager.deliveryYOffset,0);
            _flyToPlayTweenId = tween.MoveTo(gameObject, flyTargetPos, auctionItemManager.deliveryFlyDuration, 0f, tween.EaseInOutSine, false);
            if(scaleThisItemAfterBought)
                _scaleDuringFlyingTweenId = tween.LocalScaleTo(gameObject, sizeAfterBought, auctionItemManager.deliveryFlyDuration, 0f, tween.EaseInOutSine, false);
            
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

        private void CheckFlying()
        {
            if (!isFlying) return;

            if ((transform.position - flyTargetPos).magnitude > arriveDistanceThreshold) return;
            
            tween.Kill(_flyToPlayTweenId);
            if(scaleThisItemAfterBought)
                tween.Complete(_scaleDuringFlyingTweenId);
            //Arrive the target position now
            SetKinematic(false);
            isFlying = false;
            _rb.useGravity = true;
            
            if (!boughtPlaySound && boughtPlayAudioClip)
            {
                boughtPlaySound = true;
                audioSource.clip = boughtPlayAudioClip;
                audioSource.Play();
            }
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

        [Button("Clear Instances")]
        public void ClearInstances()
        {
            if (auctionItemManager.gameIsRunning)
            {
                Debug.LogWarning("Cannot clear instances in play mode");
                return;
            }
            foreach (var obj in objectPool.Pool)
            {
                DestroyImmediate(obj);
            }
            System.Array.Clear(objectPool.Pool, 0, objectPool.Pool.Length);
            objectPool.Pool = new GameObject[0];
        }

        [Button("Generate Instances")]
        public void GenerateInstances()
        {
            if (auctionItemManager.gameIsRunning)
            {
                Debug.LogWarning("Cannot clear instances in play mode");
                return;
            }
            
            if (!auctionItemManager)
            {
                Debug.LogWarning("Assign the manager first to continue generate");
                return;
            }

            if (!objectPool)
            {
                objectPool = GetComponent<VRCObjectPool>();
                if(!objectPool)
                    Debug.LogError($"There is no object pool on {name}");
            }
            objectPool.Pool = new GameObject[auctionItemManager.itemPoolSize];
            var sample = gameObject; //If directly use Instantiate(gameobject, transform) will also copy the nested children. OK NOT WORKING
            for (var i = 0; i < auctionItemManager.itemPoolSize; i++)
            {
                var obj = Instantiate(sample, transform);
                DestroyImmediate(obj.GetComponent<AuctionSample>());
                foreach (Transform child in obj.transform)
                {
                    DestroyImmediate(child.gameObject); // are there better ways?
                }
                obj.SetActive(false);
                objectPool.Pool[i] = obj;
            }
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
