
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
    [Required,InfoBox("The object to assign to each player")] 
    public GameObject objectToAssign;
    private GameObject _instantiatedObject;

    private string ObjectName {get { return objectToAssign.name; }}

    public void Start()
    {
       // if(objectToAssign.scene.IsValid()) //if it is sceneobject.
            objectToAssign.SetActive(false);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == null || player.isLocal) return;
        
        var _gameObject = Instantiate(objectToAssign,parent:transform);
        _gameObject.name = ObjectName + " of " + player.playerId;
        _instantiatedObject = _gameObject;
        Networking.SetOwner(player,_instantiatedObject);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (!_instantiatedObject) return;
        
        Destroy(_instantiatedObject);
        _instantiatedObject = null;
    }
}
