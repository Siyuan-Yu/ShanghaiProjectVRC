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
    public int playerID;

    public Text idText;
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
        idText.text = playerID.ToString();
    }
}
