using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerController : MonoBehaviour
{
    public Vector3 closedPosition; // Local position when closed
    public Vector3 openPosition;   // Local position when open
    public float moveSpeed = 2f;

    private bool isOpen = false;
    private bool isMoving = false;
    private Rigidbody rb;
    private Vector3 targetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb.isKinematic)
        {
            Debug.LogWarning("Drawer Rigidbody should be set to Kinematic.");
            rb.isKinematic = true;
        }

        // Set the initial position
        targetPosition = closedPosition;
        rb.MovePosition(transform.parent.TransformPoint(closedPosition));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.transform == transform) // only toggle if this drawer was clicked
                {
                    ToggleDrawer();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            Vector3 worldTarget = transform.parent.TransformPoint(targetPosition);
            Vector3 newPos = Vector3.MoveTowards(rb.position, worldTarget, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector3.Distance(rb.position, worldTarget) < 0.001f)
            {
                isMoving = false;
            }
        }
    }

    public void ToggleDrawer()
    {
        isOpen = !isOpen;
        targetPosition = isOpen ? openPosition : closedPosition;
        isMoving = true;
    }
}

