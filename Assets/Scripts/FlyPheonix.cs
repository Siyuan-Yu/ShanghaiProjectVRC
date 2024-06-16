
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FlyPheonix : UdonSharpBehaviour
{

    private VRCPlayerApi playerLocal;
    private bool isActive;
    
    void Start()
    {
        playerLocal = Networking.LocalPlayer;
    }

    private void OnPickupUseDown()
    {
        isActive = true;
    }

    private void OnPickupUseUp()
    {
        isActive = false;
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            playerLocal.SetVelocity((Vector3.ClampMagnitude(playerLocal.GetVelocity() + transform.forward, 5)));
        }
    }
}
