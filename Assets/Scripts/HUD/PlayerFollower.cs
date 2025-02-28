
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;
using VRC.Udon.Common;

namespace HUD
{
    public class PlayerFollower : UdonSharpBehaviour
    {
        
        private VRCPlayerApi _localPlayer;

        [Title("Player Follwer"),SerializeField]
        private bool useMotionSmoothing = true;

        [SerializeField, Range(1, 20), ShowIf("useMotionSmoothing")]
        private float motionSmoothing = 18f;

        public virtual void Start()
        {
            if (Networking.LocalPlayer.IsValid())
            {
                _localPlayer = Networking.LocalPlayer;
            }
        }

        public override void PostLateUpdate()
        {
            var head = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            transform.position = head.position;
            if (useMotionSmoothing)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, head.rotation,
                    1.0f - Mathf.Exp(-motionSmoothing * Time.deltaTime));
            }
            else
            {
                transform.rotation = head.rotation;
            }
        }

        public override void InputJump(bool value, UdonInputEventArgs args)
        {
        }
    }
}
