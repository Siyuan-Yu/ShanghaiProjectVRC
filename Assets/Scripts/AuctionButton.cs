
using System;
using Objects;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class AuctionButton : InteractAnimatorController
{
    [Space,Title("Auction Button")]
    public GameObject auctionUnit;

    [SerializeField] private bool useForAuction = true;
    [UdonSynced,ReadOnly] public int clickNum;
    [UdonSynced] public string playerName;
    
    protected override void Start()
    {
        if (conditionType == ConditionType.Trigger)
        {
            animator.ResetTrigger(triggerName);
        }
        
        if (animator) return;
        
        animator = GetComponent<Animator>();
        if (!animator)
        {
            animator = transform.parent.GetComponent<Animator>();

            if (!animator)
            {
                Debug.LogWarning($"AuctionButton {name}: animator is null on itself and its parent");
            }
        }
    }

    private void OnValidate()
    {
        if (animator) return;
        
        Debug.Log($"Trying to get Animator of {name} on itself and its parent");
        
        animator = GetComponent<Animator>();
        if (!animator)
        {
            animator = transform.parent.GetComponent<Animator>();

            if (!animator)
            {
                Debug.LogWarning($"AuctionButton {name}: animator is null on itself and its parent");
            }
        }
    }

    public override void Interact()
    {
        base.Interact();
        
        if (auctionUnit && useForAuction)
        {
            OnButtonClick();
        }
    }

    
    public void OnButtonClick()
    {
        clickNum += 1;
        playerName = Networking.LocalPlayer.displayName;
        RequestSerialization();
    }

    public void OnReset()
    {
        clickNum = 0;
        RequestSerialization();
    }
}
