
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
    private DetectionWallTrigger detectionWallTrigger;

    // public bool triggered;
    void Start()
    {
        if(wallTrigger)
            detectionWallTrigger = wallTrigger.GetComponent<DetectionWallTrigger>();

        if (!detectionWallTrigger)
        {
            Debug.LogError($"There is no DetectionWallTrigger on the wallTrigger of {name}");
        }
    }

    private void Update()
    {
        if(!detectionWallTrigger) return;

        if (!detectionWallTrigger.isTriggered) return; 
        
        if (detectionWallTrigger.isOverScore)
        {
            if (unitDoors.anim)
            {
                unitDoors.anim.SetBool("Open", true);
            }
        }

        else
        {
            if (unitDoors.anim)
            {
                detectionWallTrigger.isTriggered = false;
                unitDoors.anim.SetBool("Open", false);
            }
        }
        /*if (detectionWallTrigger.isTriggered) //sorry but These codes are literally awful and scary
        {
            if (detectionWallTrigger.isOverScore)
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
                    detectionWallTrigger.isTriggered = false;
                    unitDoors.anim.SetBool("Open", false);
                }
            }
        }*/
    }
    
}
