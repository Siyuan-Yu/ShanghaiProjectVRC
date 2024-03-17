using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ClimbingSystem : UdonSharpBehaviour
{
    public LayerMask climbableLayer; // 用于识别可攀爬表面的LayerMask
    public VRCPlayerApi playerApi;

    public bool isClimbing = false;
    public Transform handHold; // 玩家当前抓握的点

    public Vector3 simulatedHandPosition; // 模拟的手部位置

    public GameObject handHoldPrefab;
    
    void Start()
    {
        playerApi = Networking.LocalPlayer;
        if (playerApi == null)
        {
            Debug.LogWarning("ClimbingSystem: Running in non-VRChat environment, simulation mode enabled.");
            simulatedHandPosition = new Vector3(0, 1.5f, 0); // 初始化模拟手部位置
        }
    }

    void Update()
    {
        if (playerApi != null && !playerApi.IsUserInVR())
        {
            return; // 如果在VRChat中但不是VR模式，则不执行攀爬逻辑
        }

        // 使用键盘输入来模拟攀爬动作的开始和结束
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isClimbing)
            {
                StartClimbing();
            }
            else
            {
                StopClimbing();
            }
        }

        if (isClimbing)
        {
            if (handHold != null)
            {
                // 在桌面模式下，使用模拟的手部位置
                if (playerApi == null)
                {
                    // 模拟手部上下移动
                    simulatedHandPosition += new Vector3(0, Input.GetAxis("Vertical") * 0.1f, 0);
                    handHold.position = simulatedHandPosition;
                }

                // 更新玩家位置以模拟攀爬
                if (playerApi != null && handHold != null)
                {
                    playerApi.TeleportTo(handHold.position, playerApi.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.Default, false);
                }
            }
        }
        else
        {
            CheckForClimbStart();
        }
    }

    private void CheckForClimbStart()
    {
        // 这里添加检测玩家手部是否接触到可攀爬表面的逻辑
        // 在桌面模式下，始终认为可以攀爬
        if (playerApi == null)
        {
            simulatedHandPosition = new Vector3(0, 1.5f, 0); // 重置模拟手部位置
            isClimbing = true;
            if (handHoldPrefab != null)
            {
                GameObject handHoldObject = VRCInstantiate(handHoldPrefab);
                handHold = handHoldObject.transform;
            }
            handHold.position = simulatedHandPosition;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
    }

    private void StopClimbing()
    {
        isClimbing = false;
        if (handHold != null)
        {
            Destroy(handHold.gameObject);
            handHold = null;
        }
    }
}