
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System.Collections.Generic;

public class AssignPlayerInitialPos : UdonSharpBehaviour
{
    public GameObject[] allUnits;

    [SerializedField] public int[] UnitPlayerNum;

    [SerializedField] public int unitIndex;
    public int maxHoldPerUnit;

    public Text playerIDInfo;
    public VRCPlayerApi[] allPlayers = new VRCPlayerApi[20];

    void Start()
    {
        UnitPlayerNum = new int[20];
    }

    void Update()
    {
        VRC.SDKBase.VRCPlayerApi.GetPlayers(allPlayers);

        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
            playerIDInfo.text = player.displayName;
        }

        UnitPlayerNum[unitIndex] += 1;




    }

}
