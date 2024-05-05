
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RollingCoke : UdonSharpBehaviour
{
    public float speed = 5.0f;

    void Update()
    {
        // 使罐子前进并且滚动
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.forward, 360 * speed * Time.deltaTime, Space.World);
    }
}
