using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class PointSystem : UdonSharpBehaviour
{
    [UdonSynced] public int[] allPlayerIDs;
    [UdonSynced] public int[] allPoints;
    
    public int playerID;

    public int points = 0;

    [UdonSynced] public int playerNums;
    
    public VRCPlayerApi player;
    public Text[] idTexts;
    public Text[] pointTexts;
    public Text playerNumText;

    public GameObject pointShowUI;

    [UdonSynced] public string[] idStrings;
    [UdonSynced] public string[] pointStrings;
    void Start()
    {
        player = Networking.LocalPlayer;
    }

    private void Update()
    {
        playerNums = VRCPlayerApi.GetPlayerCount();
        playerNumText.text = playerNums.ToString();

        playerID = VRCPlayerApi.GetPlayerId(player);
        
        idStrings[playerID -1 % playerNums ] = playerID.ToString();
        pointStrings[playerID - 1 % playerNums] = points.ToString();

        // idTexts[playerID -1 % playerNums ].text = playerID.ToString();
        // pointTexts[playerID -1 %playerNums ].text = points.ToString();

        for (int i = 0; i < idTexts.Length; i++)
        {
            idTexts[i].text = idStrings[i];
            pointTexts[i].text = pointStrings[i];
        }
    }
}
