using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ItemBelongTest : UdonSharpBehaviour
{
    [UdonSynced] public int playerID;
    public Text idText;
    [UdonSynced] public int addPointVal;
    
    [UdonSynced] public bool canAddPoint = true;
    
    void Start()
    {
        playerID = 100;
    }
    
    public override void OnPlayerTriggerEnter(VRCPlayerApi other)
    {
        VRCPlayerApi player = other;
        // Check if the player exists and is active
        if (player != null)
        {
            playerID = VRCPlayerApi.GetPlayerId(player);
        }
    }

    private void Update()
    {
        if (!canAddPoint)
        {
            addPointVal = 0;
            transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }
        idText.text = playerID.ToString();
    }
}
