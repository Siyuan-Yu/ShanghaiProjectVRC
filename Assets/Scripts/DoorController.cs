using UnityEngine;
using System.Collections; // Add this line

public class DoorController : MonoBehaviour
{
    public Animator leftDoorAnimator; // Animator for the left door
    public Animator rightDoorAnimator; // Animator for the right door
    public float openDelay = 60f; // Time before doors open (in seconds)
    public float closeDelay = 60f; // Time before doors close (in seconds)

    private void Start()
    {
        // Start the opening and closing sequence
        StartCoroutine(OpenCloseDoors());
    }

    private IEnumerator OpenCloseDoors()
    {
        while (true)
        {
            // Wait for the specified time before opening
            yield return new WaitForSeconds(openDelay);

            // Open both doors
            leftDoorAnimator.SetTrigger("L_DoorOpen");
            rightDoorAnimator.SetTrigger("R_DoorOpen");

            // Wait for the duration of the doors being open
            yield return new WaitForSeconds(closeDelay);

            // Close both doors
            leftDoorAnimator.SetTrigger("L_DoorClose");
            rightDoorAnimator.SetTrigger("R_DoorClose");
        }
    }
}
