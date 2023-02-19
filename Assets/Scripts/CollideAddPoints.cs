
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CollideAddPoints : UdonSharpBehaviour
{
    public UdonBehaviour pointSystem;
    public int localPoint;
    void Start()
    {
        // pointSystem = PointSystem
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi col)
    {
        if (col == Networking.LocalPlayer)
        {
            localPoint = (int)pointSystem.GetProgramVariable("points");
            localPoint += 2;
            pointSystem.SetProgramVariable("points", localPoint);
        }
    }
}
