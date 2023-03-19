using TryScripts;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using VRC.Udon.Common.Interfaces;
// using UdonSharpEditor;


public class CollideAddPoints : UdonSharpBehaviour
{
    public UdonBehaviour pointSystem;
    public int localPoint;
    public int detectLayer;
    public UdonBehaviour itb;

    public Text collideIDText;
    public GameObject target;

    void Start()
    {
        // pointSystem = PointSystem
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == detectLayer)
        {
            VRCPlayerApi player = Networking.LocalPlayer;

            itb = (UdonBehaviour)other.GetComponent(typeof(UdonBehaviour));

            int point = (int)itb.GetProgramVariable("addPointVal");
            int itemOwnerPlayerID = (int)itb.GetProgramVariable("playerID");


            // point != null && point != 0 && itemOwnerPlayerID != null
            if (point != null && point != 0)
            {
                collideIDText.text = localPoint.ToString();
                
                localPoint = (int)pointSystem.GetProgramVariable("points");
                localPoint += point;
                pointSystem.SetProgramVariable("points", localPoint);

                target = other.gameObject;
                
                // string[] playerPointStrings = (string[])pointSystem.GetProgramVariable("pointStrings");
                // string playerCertainPointString = playerPointStrings[itemOwnerPlayerID - 1];
                // int playerCertainPoint = int.Parse(playerCertainPointString);
                // playerCertainPoint += point;
                // playerPointStrings[itemOwnerPlayerID - 1] = playerCertainPoint.ToString();
                // pointSystem.SetProgramVariable("pointStrings", playerPointStrings);

                // Destroy(other.gameObject);
                // other.gameObject.SetActive(false);
                SendCustomNetworkEvent(NetworkEventTarget.All, "ToggleTargetFalse");
            }
            else
            {
                collideIDText.text = "NOTHING COLLIDED!!";
            }
        }
    }

    public void ToggleTargetFalse()
    {
        target.GetComponent<Renderer>().material.color = new Color(0f,0f,0f,0f);
        target.SetActive(false);
    }
}
