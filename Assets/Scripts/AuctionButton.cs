
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

public class AuctionButton : UdonSharpBehaviour
{
    public int selfClickTime;

    public GameObject auctionUnit;
    public int nm;
    void Interact()
    {
        if (auctionUnit != null)
        {
            auctionUnit.GetComponent<UnitClickCounter>().clickNum += 1;
        }

        selfClickTime += 1;
    }

    private void Update()
    {
        if (auctionUnit != null)
        {
            nm = auctionUnit.GetComponent<UnitClickCounter>().clickNum;
        }
    }
}
