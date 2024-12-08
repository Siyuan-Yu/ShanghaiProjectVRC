
using System;
using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DetectionWallTrigger : UdonSharpBehaviour
{
    public UdonBehaviour pointSystem;
    public GameObject dayNightSystem;
    public bool isTriggered;
    public bool isOverScore;
    public int playerPoint;
    public int pointLine;

    public int storeSecond;
    public int curSecond;

    public int pointToMinus;
    void Start()
    {

    }

    private void Update()
    {
        curSecond = VRC.SDKBase.Networking.GetNetworkDateTime().Second;
        playerPoint = (int)pointSystem.GetProgramVariable("points");

        if (isOverScore)
        {
            if (curSecond >= storeSecond)
            {
                isOverScore = false;
            }
        }
        
        if (!dayNightSystem.GetComponent<DayNightEventSystem>().isDay)
        {
            isTriggered = false;
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != null)
        {
            if (dayNightSystem.GetComponent<DayNightEventSystem>().isDay)
            {
                isTriggered = true;
            }

            Debug.Log("PlayerExist");
            if (playerPoint > pointLine)
            {
                Debug.Log("DoorShouldOpen");
                storeSecond = VRC.SDKBase.Networking.GetNetworkDateTime().Second;
                if (storeSecond < 49)
                {
                    storeSecond += 10;
                }
                else
                {
                    storeSecond = 30;
                }
                
                isOverScore = true;

                playerPoint -= pointToMinus;
                pointSystem.SetProgramVariable("points", playerPoint);
            }
        }
    }
}
