
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

using UnityEngine.UI;

public class AuctionItem : UdonSharpBehaviour
{
    //被卖了
    public bool isBought = false;

    public GameObject goToUnitObject;

    public Text auctionWinnerInfoUI;

    public int startRealTimeMinute;
    public int goalRealTimeMinute;
    public int curRealTimeMinute;

    public float rotateYVal;
    public float rotateSpeed;

    public bool setKinetic = false;
    
    void Start()
    {
        rotateYVal = transform.localEulerAngles.z;
        
        startRealTimeMinute = VRC.SDKBase.Networking.GetNetworkDateTime().Minute;
        if (startRealTimeMinute < 59)
        {
            goalRealTimeMinute = startRealTimeMinute + 1;
        }
        else
        {
            goalRealTimeMinute = 0;
        }

        auctionWinnerInfoUI = GameObject.Find("AuctionWinner").GetComponent<Text>();
    }

    private void FixedUpdate()
    {
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
            GetComponent<Rigidbody>().isKinematic = true;
            rotateYVal += Time.deltaTime * rotateSpeed;
            transform.localEulerAngles = new Vector3(0, rotateYVal, 0);
            auctionWinnerInfoUI.text = "No Auction Winner Yet";
          //  Debug.Log("ROTATE");
        }
        else
        {
            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            auctionWinnerInfoUI.text = "Auction Winner Is :" + goToUnitObject.name + "  " + isBought;
            transform.position = Vector3.Lerp(transform.position, goToUnitObject.transform.position, Time.deltaTime * 0.5f);

            if (Math.Abs(transform.position.x - goToUnitObject.transform.position.x) < 3f)
            {
                if (!setKinetic)
                {
                    setKinetic = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
    }
}
