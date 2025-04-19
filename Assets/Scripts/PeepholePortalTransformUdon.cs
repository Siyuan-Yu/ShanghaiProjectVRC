using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PeepholePortalTransformUdon : UdonSharpBehaviour
{
    [Header("References")]
    public Camera portalCamera;      // The second camera rendering to the RenderTexture (e.g., MoonRenderTextureCamera)
    public Transform peephole;       // The transform at the center/orientation of the peephole (e.g., peepholeCenter)
    public Transform moonPortal;     // The virtual "exit" point that defines where the portalCamera views from

    [Header("Optional")]
    public Transform playerHead;     // (Optional) Provide your own transform for testing; otherwise will use VRChat tracking

    void LateUpdate()
    {
        if (portalCamera == null || peephole == null || moonPortal == null) return;

        Vector3 headPos;
        Quaternion headRot;

        // Get head tracking from VRChat player if available
        if (playerHead == null && Networking.LocalPlayer != null)
        {
            VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            headPos = headData.position;
            headRot = headData.rotation;
        }
        else if (playerHead != null)
        {
            headPos = playerHead.position;
            headRot = playerHead.rotation;
        }
        else
        {
            return; // No valid head reference
        }

        // Construct matrix from player's current head pose
        Matrix4x4 playerMatrix = Matrix4x4.TRS(headPos, headRot, Vector3.one);

        // Remap it through the portals
        Matrix4x4 finalMatrix =
            moonPortal.localToWorldMatrix *
            peephole.worldToLocalMatrix *
            playerMatrix;

        // Apply new transform to the portal camera
        portalCamera.transform.SetPositionAndRotation(
            finalMatrix.GetColumn(3),
            finalMatrix.rotation
        );
    }
}
