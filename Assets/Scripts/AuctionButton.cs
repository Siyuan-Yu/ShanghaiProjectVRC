
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
    void Interact()
    {
        selfClickTime += 1;
    }

    public void Update()
    {
    }
}
