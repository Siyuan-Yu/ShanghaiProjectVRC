using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PeepholeRenderActivator : UdonSharpBehaviour
{
    public Camera targetCamera;            // The camera rendering to the RenderTexture
    public Renderer peepholeRenderer;      // The peephole quad with the masked material
    public Camera viewerCamera;            // The main player camera (can be left blank)

    void Start()
    {
        Debug.Log("PeepholeRenderActivator STARTED on " + gameObject.name);

        if (viewerCamera == null && Camera.main != null)
        {
            viewerCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null || peepholeRenderer == null || viewerCamera == null) return;

        // Perform visibility check
        bool isVisible = GeometryUtility.TestPlanesAABB(
            GeometryUtility.CalculateFrustumPlanes(viewerCamera),
            peepholeRenderer.bounds
        );

        // Toggle camera based on visibility
        targetCamera.enabled = isVisible;

        // Debug message to verify what the script thinks
        Debug.Log("Peephole visible: " + isVisible);
    }
}
