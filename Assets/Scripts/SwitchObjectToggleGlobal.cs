using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class SwitchObjectToggleGlobal : UdonSharpBehaviour
{
    // 初期状態でActiveなオブジェクトはactiveObjectsに
    // 初期状態でInactiveなオブジェクトはinactiveObjectsに
    // それぞれ登録。Interactする度に切り替わる。
    [SerializeField]
    private GameObject[] activeObjects, inactiveObjects;

    private bool _isInitialized = false, _isSwitched = false;

    public override void Interact()
    {
        if(_isSwitched)
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetActiveTrue));
        else
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(SetActiveFalse));
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.LocalPlayer.isMaster) return;

        if (_isSwitched)
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(InitializeSwitched));
    }

    public void InitializeSwitched()
    {
        if (_isInitialized) return;
        SendCustomEvent(nameof(SetActiveFalse));
        _isInitialized = true;
    }

    public void SetActiveTrue()
    {
        foreach (var go in activeObjects) go.SetActive(true);
        foreach (var go in inactiveObjects) go.SetActive(false);
        _isSwitched = false;
    }

    public void SetActiveFalse()
    {
        foreach (var go in activeObjects) go.SetActive(false);
        foreach (var go in inactiveObjects) go.SetActive(true);
        _isSwitched = true;
    }
}