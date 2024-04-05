
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ClimbingLadder : UdonSharpBehaviour
{
    public Transform topOfLadder;
    public VRCPlayerApi player;
    [UdonSynced] public bool isClimbing = false;
    private float climbStartTime;
    public float climbDuration = 5.0f; // 攀爬到顶部所需的时间
    private Vector3 climbStartPos;

    private void Start()
    {
        player = Networking.LocalPlayer;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi enteringPlayer)
    {
        if (enteringPlayer.isLocal)
        {
            StartClimbing();
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        climbStartTime = Time.time;
        climbStartPos = player.GetPosition(); // 获取攀爬开始时的玩家位置
        player.SetVelocity(Vector3.zero); // 停止玩家的当前运动
    }

    private void Update()
    {
        if (isClimbing)
        {
            float elapsedTime = Time.time - climbStartTime;
            float progress = elapsedTime / climbDuration;

            if (progress < 1.0f)
            {
                // 插值计算玩家当前位置
                Vector3 currentPosition = Vector3.Lerp(climbStartPos, topOfLadder.position, progress);
                player.TeleportTo(currentPosition, player.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.Default, false);
            }
            else
            {
                // 攀爬结束
                isClimbing = false;
            }
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi exitingPlayer)
    {
        if (exitingPlayer.isLocal && isClimbing)
        {
            // 玩家离开梯子时停止攀爬
            isClimbing = false;
        }
    }
}
