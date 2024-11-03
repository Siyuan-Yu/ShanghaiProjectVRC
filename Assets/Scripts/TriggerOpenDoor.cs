
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TriggerOpenDoor : UdonSharpBehaviour
{
    public GameObject doorToTrigger;
    public bool isOn;
    public Animator anim;
    void Start()
    {
        anim = doorToTrigger.GetComponent<Animator>();
    }

    private void Update()
    {
        if (isOn)
        {
            anim.SetBool("Open", true);
        }
        else
        {
            anim.SetBool("Open", false);
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi enteringPlayer)
    {
        if (enteringPlayer.isLocal)
        {
            isOn = true;
        }
    }

    public override void OnPlayerTriggerStay(VRCPlayerApi enteringPlayer)
    {
        if (enteringPlayer.isLocal)
        {
            isOn = true;
        }
    }
    
    public override void OnPlayerTriggerExit(VRCPlayerApi enteringPlayer)
    {
        if (enteringPlayer.isLocal)
        {
            isOn = false;
        }
    }
}
