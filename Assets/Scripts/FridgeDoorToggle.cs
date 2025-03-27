using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FridgeDoorToggle : UdonSharpBehaviour
{
    public Animator doorAnimator;
    private bool isOpen = false;

    public override void Interact()
    {
        //self.InteractionText = "Switch";
        isOpen = !isOpen;
        doorAnimator.SetBool("isOpen", isOpen);
    }
}
