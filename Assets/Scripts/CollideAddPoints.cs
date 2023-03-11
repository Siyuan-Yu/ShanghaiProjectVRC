using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
// using UdonSharpEditor;


public class CollideAddPoints : UdonSharpBehaviour
{
    public UdonBehaviour pointSystem;
    public int localPoint;
    public int detectLayer;
    public UdonBehaviour itb; 
    void Start()
    {
        // pointSystem = PointSystem
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == detectLayer)
        {
            VRCPlayerApi player = Networking.LocalPlayer;
            
            itb = (UdonBehaviour) other.GetComponent(typeof(UdonBehaviour));
            
            int point = (int)itb.GetProgramVariable("addPointVal");
            int itemOwnerPlayerID = (int)itb.GetProgramVariable("playerID");

            if (point != null && point != 0 && itemOwnerPlayerID != null)
            {
                string[] playerPointStrings = (string[])pointSystem.GetProgramVariable("pointStrings");
                string playerCertainPointString = playerPointStrings[itemOwnerPlayerID];
                int playerCertainPoint = int.Parse(playerCertainPointString);
                // localPoint = (int)pointSystem.GetProgramVariable("points");
                // (int)pointSystem.GetProgramVariable("points");
                playerCertainPoint += point;
                playerPointStrings[itemOwnerPlayerID] = playerCertainPoint.ToString();
                pointSystem.SetProgramVariable("pointStrings", playerPointStrings);
                Destroy(other.gameObject);
            }
        }
    }
    
}
