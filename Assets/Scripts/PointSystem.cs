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
    public int[] allPlayerIDs;
    public int[] allPoints;
    
    public int playerID;

    public int points = 0;

    public int playerNums;
    
    public VRCPlayerApi player;
    public Text[] idTexts;
    public Text[] pointTexts;
    public Text playerNumText;

    public GameObject pointShowUI;
    void Start()
    {
        // var ahha = VRCInstantiate(idText.gameObject);
        // ahha.transform.position = pointShowUI.transform.position;
        // ahha.transform.rotation = pointShowUI.transform.rotation;
        //
        // var ahhaasf = VRCInstantiate(pointText.gameObject);
        // ahhaasf.transform.position = new Vector3(pointShowUI.transform.position.x, pointShowUI.transform.position.y-200f, pointShowUI.transform.position.z);
        // ahhaasf.transform.rotation = pointShowUI.transform.rotation;
        
        player = Networking.LocalPlayer;
    }

    private void Update()
    {
        
        playerID = VRCPlayerApi.GetPlayerId(player);
        idTexts[playerID-1].text = playerID.ToString();
        pointTexts[playerID-1].text = points.ToString();
        
        
        playerNums = VRCPlayerApi.GetPlayerCount();
        playerNumText.text = playerNums.ToString();


        // allPlayerIDs = new int[playerNums];
    }
}
