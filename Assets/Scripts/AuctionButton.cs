
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

public class AuctionButton : UdonSharpBehaviour
{
    public GameObject auctionSystem;
    public int unitID;
    public int selfClickTime;

    public GameObject auctionUnit;
    void Interact()
    {
        auctionUnit.GetComponent<UnitClickCounter>().clickNum += 1;
        selfClickTime += 1;
    }
}
