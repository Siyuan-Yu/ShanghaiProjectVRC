
using System;
using System.Collections;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DetectionWall : UdonSharpBehaviour
{
    public BoxCollider col;
    public UdonBehaviour wallTrigger;


    // public bool triggered;
    void Start()
    {

    }

    private void Update()
    {

        if ((bool)wallTrigger.GetProgramVariable("isTriggered") == true)
        {
            col.enabled = false;
        }
        else
        {
            col.enabled = true;
        }
        
        

        // SendCustomEventDelayedSeconds("ReCoverWall", 10f);
    }
    
}
