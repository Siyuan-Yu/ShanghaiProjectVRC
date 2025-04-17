using System;
using Objects;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.Scripting;
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
        if(!animator)
            animator = GetComponent<Animator>();
        if (!animator)
        {
            animator = transform.parent.GetComponent<Animator>();

            if (!animator)
            {
                Debug.LogWarning($"AuctionButton {name}: animator is null on itself and its parent");
            }
        }
        
        if (!animator) return;

        if (conditionType == ConditionType.Trigger)
        {
            animator.ResetTrigger(triggerName);
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
        if(Networking.GetOwner(gameObject) != Networking.LocalPlayer)
        {
            // Take ownership of this button
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            Debug.Log($"Transfer {transform.parent.name}'s {name}'s owner to {Networking.LocalPlayer}");
            
            // Small delay to ensure ownership transfer completes
            SendCustomEventDelayedSeconds("PerformButtonClick", 0.1f);
            
            playerName = Networking.LocalPlayer.displayName;
        }
        else
        {
            if(playerName == "")
                playerName = Networking.LocalPlayer.displayName;
            PerformButtonClick();
        }
    }
    
    [Preserve]
    public void PerformButtonClick()
    {
        clickNum += 1;
        RequestSerialization();
    }

    public void OnReset()
    {
        clickNum = 0;
        RequestSerialization();
    }
}