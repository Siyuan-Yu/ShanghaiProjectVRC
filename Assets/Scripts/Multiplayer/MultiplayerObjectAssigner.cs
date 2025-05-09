using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class MultiplayerObjectAssigner : UdonSharpBehaviour
{
    /*[Serializable]
    public class AssignableObject
    {
        public bool hideFromStart = true;
        public GameObject gameObject;
    }*/ //not supported
    
    [Required,HorizontalGroup("Objects To Assign")] 
    public GameObject[] objectsToAssign;
    
    [HorizontalGroup("Objects To Assign")]
    public bool[] hideObjectsFromStart;

    [HideInInspector]
    public GameObject objectsAssigned;

    private void Start()
    {
        if(hideObjectsFromStart.Length != objectsToAssign.Length)
        {
            Debug.LogError("The length of objectsToAssign is diff from hideObjectsFromStart.Length");
            return;
        }
        for (int i = 0; i < objectsToAssign.Length; i++)
        {
            if(hideObjectsFromStart[i])
                objectsAssigned = objectsToAssign[i];
        }
        /*// Hide the template objects
        foreach (var obj in objectsToAssign)
        {
            if (obj)
                if(obj.hideFromStart)
                    obj.gameObject.SetActive(false);
        }*/
    }

    /*public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == null || player.isLocal) return;
        
        
        // Instantiate all objects for this player
        for (var i = 0; i < objectsToAssign.Length; i++)
        {
            if (!objectsToAssign[i]) continue;
            
            newObject.name = objectsToAssign[i].name + " of " + player.playerId;
            newObject.SetActive(true);
            
            instantiatedObjectsPerPlayer[playerIndex][i] = newObject;
            Networking.SetOwner(player, newObject);
        }
    }*/
    
    
}

