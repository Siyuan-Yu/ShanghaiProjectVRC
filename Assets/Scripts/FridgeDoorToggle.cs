using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

public class FridgeDoorToggle : UdonSharpBehaviour
{
    public Animator doorAnimator;
    private bool isOpen = false;

    public override void Interact()
    {
        isOpen = !isOpen;
        doorAnimator.SetBool("isOpen", isOpen);
    }
}
