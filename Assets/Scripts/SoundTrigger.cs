using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SoundTrigger : UdonSharpBehaviour
{
    public GameObject eventTargetEnter;
    public string methodNameEnter;

    public GameObject eventTargetExit;
    public string methodNameExit;

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.name);
        if (eventTargetEnter != null && !string.IsNullOrEmpty(methodNameEnter))
        {
            eventTargetEnter.SendMessage(methodNameEnter, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (eventTargetExit != null && !string.IsNullOrEmpty(methodNameExit))
        {
            eventTargetExit.SendMessage(methodNameExit, SendMessageOptions.DontRequireReceiver);
        }
    }
}
