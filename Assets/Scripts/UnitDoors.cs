
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UnitDoors : UdonSharpBehaviour
{
    [UdonSynced, FieldChangeCallback(nameof(CanStartDayNight))] 
    public bool _canStartDayNight = false;

    public Animator anim;
    public bool CanStartDayNight
    {
        get => _canStartDayNight;
        set
        {
            _canStartDayNight = value;
            // 这里可以添加一些当值改变时触发的逻辑
        }
    }

    private float activationTime = 0;
    private bool isWaiting = false;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        activationTime = Time.time + 2.0f; // 设置未来激活时间为当前时间加两秒
        isWaiting = true;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isWaiting && Time.time >= activationTime)
        {
            isWaiting = false;
            CanStartDayNight = true;
        }
    }
}
