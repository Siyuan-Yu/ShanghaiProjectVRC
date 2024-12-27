using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PointSystem : UdonSharpBehaviour
{
    [Title("Synced data")]
    [UdonSynced] public int[] allPlayerIDs;
    [UdonSynced] public int[] allPoints;
    
    [Title("Local Data")]
    [ReadOnly]public int playerID;

    public int points = 0;

    [UdonSynced] public int playerNums;
    
    public VRCPlayerApi player;
    public Text[] idTexts;
    public Text[] pointTexts;
    public Text playerNumText;

    [UdonSynced] public string[] idStrings;
    [UdonSynced] public string[] pointStrings;
    void Start()
    {
        player = Networking.LocalPlayer;
        
        playerNums = VRCPlayerApi.GetPlayerCount();
        playerNumText.text = playerNums.ToString();

        playerID = VRCPlayerApi.GetPlayerId(player);
        
        idStrings[playerID -1 % playerNums ] = playerID.ToString();
        pointStrings[playerID - 1 % playerNums] = points.ToString();
    }

    private void Update()
    {
        playerNums = VRCPlayerApi.GetPlayerCount();
        playerNumText.text = playerNums.ToString();
        //
        playerID = VRCPlayerApi.GetPlayerId(player);
        //
        // idStrings[playerID -1 % playerNums ] = playerID.ToString();
        // pointStrings[playerID - 1 % playerNums] = points.ToString();

        // idStrings[playerID - 1] = playerID.ToString();
        // pointStrings[playerID - 1] = points.ToString();
        
        idTexts[playerID - 1].text = playerID.ToString();
        pointTexts[playerID - 1].text = points.ToString();
        
        // for (int i = 0; i < idTexts.Length; i++)
        // {
        //     idTexts[i].text = idStrings[i];
        //     pointTexts[i].text = pointStrings[i];
        // }
    }
}
