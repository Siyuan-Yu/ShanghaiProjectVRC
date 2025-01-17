
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UnitClickCounter : UdonSharpBehaviour
{
    [UdonSynced] public int clickNum;
    [Required]public GameObject auctionDeliverPos;
    
    //TODO 
    [UdonSynced] public string playerName;
}
