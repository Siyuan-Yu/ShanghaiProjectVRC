
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HudLogController : UdonSharpBehaviour
{

    public TextMeshProUGUI ownerText;

    private void Update()
    {
        ownerText.text = UpdateMaster();
    }
    
    public string UpdateMaster()
    {
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);

        foreach (var player in players)
        {
            if (player.isMaster)
            {
                return player.playerId + " " + player.displayName;
            }
        }

        return "No Master";
    }
}
