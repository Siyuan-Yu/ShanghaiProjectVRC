using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    // //stores a reference to the waypoint system this object will use
    // [SerializeField] private Waypoints waypoints;

    // [SerializeField] private float moveSpeed = 5f;

    // //the current waypoint moving to
    // private Transform currentWaypoint;


    // // Start is called before the first frame update
    // void Start()
    // {
    //    // set initial position to the 1st wp
    //     currentWaypoint = waypoints.GetNextWaypoint();
    //     transform.position = currentWaypoint.position;

    //     //set the next wp target
    //     currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //   transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

    // }
}
