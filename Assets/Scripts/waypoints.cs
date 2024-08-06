using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waypoints : MonoBehaviour
{
 private void OnDrawGizmos()
 {
    foreach(Transform t in transform)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(t.position, 1f);

    }

    // Gizmos.color = Color.red;
    // for (int i = 0; i <transform.childCount - 1; i ++)
    // {
    //     Gizmos.Drawline(transform.GetChild(i).position, transform.GetChild(i +1).position);
    // }

    // Gizmos.Drawline(transform.GetChild(transform.childCount - 1).position, transform.GetChild(0).position);
 }

}
