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

[Obsolete("To have a easier use of Auction Item, use Action/ActionItem")]
public class AuctionItem : UdonSharpBehaviour
{
    //被卖了
    [UdonSynced] public bool isBought = false;
    [UdonSynced] public bool boughtPlaySound = false;
    public GameObject AllUnits;
    public GameObject[] allUnits;

    public int goToUnitIndex;
    public bool setRandom = false;

    public Text auctionWinnerInfoUI;
    
    public float rotateYVal;
    public float rotateSpeed;

    public bool setKinetic = false;

    public bool isBigItem = false; // 大物品标识，默认为 false 表示小物品

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

    public GameObject goToUnit;
    
    [Header("音频文件")] 
    public AudioSource audioSource;
    public AudioClip auctionCountdown;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
            if (!isBigItem) // 如果不是大物品，才执行缩小逻辑
            {
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // 小物品缩小
            }
            // 对于大物品，保持原始大小，不缩小
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
        else if (isBought)
        {
            Vector3 targetPos = defaultDeliverPlace.transform.position; // 在这里声明 targetPos
            if (!boughtPlaySound)
            {
                boughtPlaySound = true;
                audioSource.clip = auctionCountdown;
                audioSource.Play();
            }
            if (!setKinetic)
            {
                if (!setRandom)
                {
                    setRandom = true;
                    ChooseRandomUnitWithValidButtonTime();
                }

                if (!isBigItem) // 如果是小物品
                {
                    Vector3 newScale = mutiVal * originalScale;
                    transform.localScale = newScale; // 小物品递送时缩小
                }
                else
                {
                    transform.localScale = originalScale; // 大物品保持原始大小
                }


                // 检查 validUnits 是否为空
                if (validUnits.Length > 0)
                {
                    auctionWinnerInfoUI.text = "Auction Winner Is :" + validUnits[goToUnitIndex].name + "  " + isBought;
                    targetPos = validUnits[goToUnitIndex].GetComponent<UnitClickCounter>().auctionDeliverPos.transform.position;
                }
                else
                {
                    // 使用 defaultDeliverPlace 的位置
                    auctionWinnerInfoUI.text = "No Valid Units. Delivering to Default Location.";
                    targetPos = defaultDeliverPlace.transform.position;
                }

                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 0.5f);
            }

            if (Math.Abs(transform.position.x - targetPos.x) < 3f)
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

    public GameObject[] validUnits;
    public int validUnitCount = 0;
    public GameObject defaultDeliverPlace;
    
    private void ChooseRandomUnitWithValidButtonTime()
    {
        // 计算有效单位数量
        // int validUnitCount = 0;
        foreach (var unit in allUnits)
        {
            UnitClickCounter unitComponent = unit.GetComponent<UnitClickCounter>();
            if (unitComponent != null && unitComponent.clickNum > 0)
            {
                validUnitCount++;
            }
        }

        if (validUnitCount > 0)
        {
            validUnits = new GameObject[validUnitCount];
            int index = 0;
        
            foreach (var unit in allUnits)
            {
                UnitClickCounter unitComponent = unit.GetComponent<UnitClickCounter>();
                if (unitComponent != null && unitComponent.clickNum > 0)
                {
                    validUnits[index] = unit;
                    index++;
                }
            }
            goToUnitIndex = UnityEngine.Random.Range(0, validUnits.Length);
        }
        else
        {
            goToUnitIndex = 0; // 或者其他逻辑
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
