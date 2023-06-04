
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DetectionWallTrigger : UdonSharpBehaviour
{
    public UdonBehaviour pointSystem;
    public bool isTriggered;
    public int playerPoint;
    public int pointLine;

    public int storeSecond;
    public int curSecond;
    void Start()
    {

    }

    private void Update()
    {
        curSecond = VRC.SDKBase.Networking.GetNetworkDateTime().Second;
        playerPoint = (int)pointSystem.GetProgramVariable("points");

        if (isTriggered)
        {
            // if (storeSecond <= 30 && curSecond > 49)
            // {
            if (curSecond >= storeSecond)
            {
                isTriggered = false;
            }
            // }
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != null)
        {
            if (playerPoint > pointLine)
            {
                
                storeSecond = VRC.SDKBase.Networking.GetNetworkDateTime().Second;
                if (storeSecond < 49)
                {
                    storeSecond += 10;
                }
                else
                {
                    storeSecond = 30;
                }
                
                isTriggered = true;

                playerPoint -= 8;
                pointSystem.SetProgramVariable("points", playerPoint);
            }
        }
    }
}
