using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class UnitDoors : UdonSharpBehaviour
{
    /*[UdonSynced, FieldChangeCallback(nameof(CanStartDayNight))]
    public bool _canStartDayNight = false;
    public bool CanStartDayNight
        {
            get => _canStartDayNight;
            set
            {
                _canStartDayNight = value;
                // 这里可以添加一些当值改变时触发的逻辑
            }
        }*/ //Shengyang: Failed to understand what is "CanStartDayNight"...
    [FormerlySerializedAs("anim")] public Animator animator;

    [UdonSynced] private bool _doorOpen = false;

    private void Start()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
            if (!animator)
                Debug.LogWarning($"No animator component attached on {name}");
        }
    }

    public void ChangeDoorState()
    {
        _doorOpen = !_doorOpen;
        ChangeDoorAnimator();
        RequestSerialization();
    }

    public void CloseDoor()
    {
        _doorOpen = false;
        ChangeDoorAnimator();
        RequestSerialization();
    }

    public void OpenDoor()
    {
        _doorOpen = true;
        ChangeDoorAnimator();
        RequestSerialization();
    }

    private void ChangeDoorAnimator()
    {
        animator.SetBool("Open", _doorOpen);
    }

    public override void OnDeserialization()
    {
        animator.SetBool("Open", _doorOpen);
    }
}
/*private float activationTime = 0;
private bool isWaiting = false;

public override void OnPlayerTriggerEnter(VRCPlayerApi player)
{
    activationTime = Time.time + 2.0f; // 设置未来激活时间为当前时间加两秒
    isWaiting = true;
}

private void Start()
{
    animator = GetComponent<Animator>();
}

private void Update()
{
    if (isWaiting && Time.time >= activationTime)
    {
        isWaiting = false;
    }
}*/