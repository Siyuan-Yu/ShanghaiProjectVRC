using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;


public class CollideAddPoints : UdonSharpBehaviour
{
    public UdonBehaviour pointSystem;
    public int localPoint;
    public int detectLayer;
    
    void Start()
    {
        // pointSystem = PointSystem
    }

    // public override void OnPlayerTriggerEnter(VRCPlayerApi col)
    // {
    //     // if (col == Networking.LocalPlayer)
    //     // {
    //         localPoint = (int)pointSystem.GetProgramVariable("points");
    //         localPoint += 2;
    //         pointSystem.SetProgramVariable("points", localPoint);
    //     // }
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == detectLayer)
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            localPoint = (int)pointSystem.GetProgramVariable("points");
            localPoint += 2;
            pointSystem.SetProgramVariable("points", localPoint);
            Destroy(other.gameObject);
        }
    }
    
}
