using Sirenix.OdinInspector;
using UdonSharp;
using UnityEngine;
using UnityEngine.Video;
using VRC.SDKBase;
using VRC.Udon;

namespace Events
{
   // [RequireComponent(typeof(VideoPlayer))]
    public class VideoPlayerEvents : UdonSharpBehaviour
    {
        /* private VideoPlayer videoPlayer;
         [SerializeField, ReadOnly] string[] publicMethodsNames = { "PlayVideo", "PauseVideo", "StopVideo" };

         private void Start()
         {
             videoPlayer = GetComponent<VideoPlayer>();
             if (videoPlayer) return;
             Debug.LogError("VideoPlayer not found");
         }

         public void PlayVideo()
         {
             videoPlayer.Play();
         }

         public void PauseVideo()
         {
             videoPlayer.Pause();
         }

         public void StopVideo()
         {
             videoPlayer.Stop();
         }*/ //Nope, video player not working 
     }
}