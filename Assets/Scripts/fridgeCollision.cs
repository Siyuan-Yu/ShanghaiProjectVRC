
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class fridgeCollision : UdonSharpBehaviour

  
{
    // public Collider collider;


    void onTriggerEnter(Collider other)
    {
       // Collider otherCollider = collision.collider;

       // if(othercollider == collider)
       // {
            Debug.Log("collision happens!");
       // }
    }

    void OnTriggerExit(Collider other)
    {
       // Collider otherCollider = collision.collider;

       // if(otherCollider == collider)
       // {
            Debug.Log("stop collision!");
        //}
    }

    void Update()
    {
       // Debug.Log("itis running");
    }


}
