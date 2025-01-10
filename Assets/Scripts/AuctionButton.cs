
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

    // Reference to the Animator component
    public Animator buttonAnimator;

    // Trigger name for the button animation
    public string animationTrigger = "pushed";

    public override void Interact()
    {
        // Play the button animation when the player interacts
        if (buttonAnimator != null)
        {
            buttonAnimator.SetTrigger(animationTrigger);  // Trigger the animation
        }

        // Handle interaction with auction unit (if present)
        if (auctionUnit != null)
        {
            auctionUnit.GetComponent<UnitClickCounter>().clickNum += 1;
        }

        selfClickTime += 1;
    }

    private void Update()
    {
        // Update the number of clicks from the auction unit
        if (auctionUnit != null)
        {
            nm = auctionUnit.GetComponent<UnitClickCounter>().clickNum;
        }
    }
}
