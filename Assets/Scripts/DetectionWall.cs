
using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DetectionWall : UdonSharpBehaviour
{
    public UnitDoors unitDoors;
    public GameObject wallTrigger;

    // public bool triggered;
    void Start()
    {

    }

    private void Update()
    {
        if (wallTrigger.GetComponent<DetectionWallTrigger>().isTriggered)
        {
            if (wallTrigger.GetComponent<DetectionWallTrigger>().isOverScore)
            {
                if (unitDoors.anim != null)
                {
                    unitDoors.anim.SetBool("Open", true);
                }
            }

            else
            {
                if (unitDoors.anim != null)
                {
                    wallTrigger.GetComponent<DetectionWallTrigger>().isTriggered = false;
                    unitDoors.anim.SetBool("Open", false);
                }
            }
        }
    }
    
}
