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
    public int playerID;

    public int points = 0;
    
    public VRCPlayerApi player;
    public Text idText;
    public Text pointText;
    void Start()
    {
        player = Networking.LocalPlayer;
    }

    private void Update()
    {
        playerID = VRCPlayerApi.GetPlayerId(player);
        idText.text = playerID.ToString();
        pointText.text = points.ToString();
    }
}
