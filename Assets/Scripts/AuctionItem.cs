using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using VRC.Udon.Common.Interfaces;
// using Random = UnityEngine.Random;

public class AuctionItem : UdonSharpBehaviour
{
    //被卖了
    [UdonSynced] public bool isBought = false;
    
    public GameObject AllUnits;
    public GameObject[] allUnits;

    public int goToUnitIndex;
    public bool setRandom = false;

    public Text auctionWinnerInfoUI;
    
    public float rotateYVal;
    public float rotateSpeed;

    public bool setKinetic = false;

    public Vector3 originalScale;
    public float mutiVal;
    [UdonSynced] public int playerID;
    public Text idText;
    [UdonSynced] public int addPointVal;
    
    [UdonSynced] public bool canAddPoint = true;

    public UdonBehaviour pointSystem;
    public int localPoint;

    public Text collideIDText;

    public bool isPheonix;
    
    void Start()
    {
        Transform[] childTransforms = AllUnits.GetComponentsInChildren<Transform>();
        allUnits = new GameObject[childTransforms.Length - 1];  
        int index = 0;
        foreach (Transform child in childTransforms)
        {
            if (child != AllUnits.transform)
            {
                allUnits[index] = child.gameObject;
                index++;
            }
        }
        
        mutiVal = 0.05f;
        playerID = 100;

        originalScale = transform.localScale;
        
        rotateYVal = transform.localEulerAngles.z;
        
    }

    private void FixedUpdate()
    {
        if (!canAddPoint && !isPheonix)
        {
            addPointVal = 0;
            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
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
                if (!setRandom)
                {
                    setRandom = true;
                    ChooseRandomUnitWithValidButtonTime();
                }
                
                Vector3 newScale = mutiVal * originalScale;
                transform.localScale = newScale;
                // transform.localScale = originalScale;
                auctionWinnerInfoUI.text = "Auction Winner Is :" + allUnits[goToUnitIndex].name + "  " + isBought;
                Vector3 targetPos = allUnits[goToUnitIndex].GetComponent<UnitClickCounter>().auctionDeliverPos.transform.position;
                transform.position = Vector3.Lerp(transform.position, targetPos,
                    Time.deltaTime * 0.5f);
            }

            if (Math.Abs(transform.position.x - allUnits[goToUnitIndex].GetComponent<UnitClickCounter>().auctionDeliverPos.transform.position.x) < 3f)
            {
                if (!setKinetic)
                {
                    if (isPheonix)
                    {
                        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    }
                    
                    setKinetic = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                }
            }
        }
    }
    
    private void ChooseRandomUnitWithValidButtonTime()
    {
        bool validUnitFound = false;
        
        while (!validUnitFound)
        {
            // 随机选择一个index
            goToUnitIndex = UnityEngine.Random.Range(0, allUnits.Length);

            // 获取Unit组件
            UnitClickCounter unitComponent = allUnits[goToUnitIndex].GetComponent<UnitClickCounter>();
            
            // 检查buttonTime是否大于0
            if (unitComponent != null && unitComponent.clickNum > 0)
            {
                validUnitFound = true;
            }
        }
    }
    
    
    public override void OnPlayerTriggerEnter(VRCPlayerApi other)
    {
        VRCPlayerApi player = other;
        // Check if the player exists and is active
        if (player != null && !isPheonix)
        {
            GetComponent<Rigidbody>().isKinematic = false;
            playerID = VRCPlayerApi.GetPlayerId(player);

            if (canAddPoint)
            {
                if (addPointVal != 0 && addPointVal != null)
                {
                    collideIDText.text = localPoint.ToString();
                    localPoint = (int)pointSystem.GetProgramVariable("points");

                    localPoint += addPointVal;
                    pointSystem.SetProgramVariable("points", localPoint);
                    
                }
                canAddPoint = false;
            }
        }
    }
}
