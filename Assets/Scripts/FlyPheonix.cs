
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class FlyPheonix : UdonSharpBehaviour
{

    private VRCPlayerApi playerLocal;
    private bool isActive;
    public Rigidbody rig;
    void Start()
    {
        rig = GetComponent<Rigidbody>();
        playerLocal = Networking.LocalPlayer;
    }

    private void OnPickupUseDown()
    {
        rig.constraints = RigidbodyConstraints.None;
        isActive = true;
    }

    private void OnPickupUseUp()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
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
