
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
    public GameObject auctionUnit;
    
    [UdonSynced,ReadOnly] public int clickNum;
    [UdonSynced] public string playerName;
    
    private void Start()
    {
        //if(auctionUnit)
    }

    public override void Interact()
    {
        base.Interact();
        
        if (auctionUnit)
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
