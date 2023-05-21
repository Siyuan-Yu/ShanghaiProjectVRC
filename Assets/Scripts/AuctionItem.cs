using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class AuctionItem : UdonSharpBehaviour
{
    //被卖了
    [UdonSynced] public bool isBought = false;

    public GameObject[] allUnitObjects;
    public int goToUnitIndex; 

    public Text auctionWinnerInfoUI;

    public int startRealTimeMinute;
    public int goalRealTimeMinute;
    public int curRealTimeMinute;

    public float rotateYVal;
    public float rotateSpeed;

    public bool setKinetic = false;

    public Vector3 originalScale;
    public float mutiVal;
    [UdonSynced] public int playerID;
    public Text idText;
    [UdonSynced] public int addPointVal;
    
    [UdonSynced] public bool canAddPoint = true;

    
    void Start()
    {
        mutiVal = 0.05f;
        playerID = 100;

        originalScale = transform.localScale;
        
        rotateYVal = transform.localEulerAngles.z;
        
        // startRealTimeMinute = VRC.SDKBase.Networking.GetNetworkDateTime().Minute;
        // if (startRealTimeMinute < 59)
        // {
        //     goalRealTimeMinute = startRealTimeMinute + 1;
        // }
        // else
        // {
        //     goalRealTimeMinute = 0;
        // }

        // auctionWinnerInfoUI = GameObject.Find("AuctionWinner").GetComponent<Text>();
    }

    private void FixedUpdate()
    {
        if (!canAddPoint)
        {
            addPointVal = 0;
            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
        // idText.text = playerID.ToString();
        
        curRealTimeMinute = VRC.SDKBase.Networking.GetNetworkDateTime().Minute;
        
        if (startRealTimeMinute < 59)
        {
            if (curRealTimeMinute >= goalRealTimeMinute)
            {
                isBought = true;
            }
            else
            {
                isBought = false;
            }
        }

        if (!isBought)
        {
            transform.localScale = originalScale;
            // transform.localScale = new Vector3(500f, 500f, 500f);
            GetComponent<Rigidbody>().isKinematic = true;
            rotateYVal += Time.deltaTime * rotateSpeed;
            transform.localEulerAngles = new Vector3(0, rotateYVal, 0);
            auctionWinnerInfoUI.text = "No Auction Winner Yet";
          //  Debug.Log("ROTATE");
        }
        else if(isBought)
        {
            if (!setKinetic)
            {
                Vector3 newScale = mutiVal * originalScale;
                transform.localScale = newScale;
                // transform.localScale = originalScale;
                auctionWinnerInfoUI.text = "Auction Winner Is :" + allUnitObjects[goToUnitIndex].name + "  " + isBought;
                transform.position = Vector3.Lerp(transform.position, allUnitObjects[goToUnitIndex].transform.position,
                    Time.deltaTime * 0.5f);
            }

            if (Math.Abs(transform.position.x - allUnitObjects[goToUnitIndex].transform.position.x) < 3f)
            {
                if (!setKinetic)
                {
                    setKinetic = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
    }
    
    public override void OnPlayerTriggerEnter(VRCPlayerApi other)
    {
        VRCPlayerApi player = other;
        // Check if the player exists and is active
        if (player != null)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            playerID = VRCPlayerApi.GetPlayerId(player);
        }
    }
}
